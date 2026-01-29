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
    public class SQA_RegPlan
    {
        private readonly SqlConnection _connection;

        public SQA_RegPlan(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_RegPlan_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_RegPlan(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                var insertQuery = @"INSERT INTO RegPlan (
                        RegPlanID, WFScenarioName, WFProfileName, WFTimeName, ActID, Entity, ApprLevelID, 
                        RegID1, RegID2, RegID, Attr1, Attr2, Attr3, Attr4, Attr5, 
                        Attr6, Attr7, Attr8, Attr9, Attr10, Attr11, Attr12, Attr13, Attr14, 
                        Attr15, Attr16, Attr17, Attr18, Attr19, Attr20, AttrVal1, AttrVal2, 
                        AttrVal3, AttrVal4, AttrVal5, AttrVal6, AttrVal7, AttrVal8, AttrVal9, 
                        AttrVal10, AttrVal11, AttrVal12, DateVal1, DateVal2, DateVal3, DateVal4, DateVal5, 
                        SpreadAmount, SpreadCurve, Status, Invalid, CreateDate, CreateUser, UpdateDate, UpdateUser
                    ) VALUES (
                        @RegPlanID, @WFScenarioName, @WFProfileName, @WFTimeName, @ActID,@Entity, @ApprLevelID, 
                        @RegID1, @RegID2, @RegID, @Attr1, @Attr2, @Attr3, @Attr4, @Attr5, 
                        @Attr6, @Attr7, @Attr8, @Attr9, @Attr10, @Attr11, @Attr12, @Attr13, @Attr14, 
                        @Attr15, @Attr16, @Attr17, @Attr18, @Attr19, @Attr20, @AttrVal1, @AttrVal2, 
                        @AttrVal3, @AttrVal4, @AttrVal5, @AttrVal6, @AttrVal7, @AttrVal8, @AttrVal9, 
                        @AttrVal10, @AttrVal11, @AttrVal12, @DateVal1, @DateVal2, @DateVal3, @DateVal4, @DateVal5, 
                        @SpreadAmount, @SpreadCurve, @Status, @Invalid, @CreateDate, @CreateUser, @UpdateDate, @UpdateUser)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@RegPlanID", SqlDbType.UniqueIdentifier).SourceColumn = "RegPlanID";
                sqa.InsertCommand.Parameters.Add("@WFScenarioName", SqlDbType.NVarChar, 100).SourceColumn = "WFScenarioName";
                sqa.InsertCommand.Parameters.Add("@WFProfileName", SqlDbType.NVarChar, 100).SourceColumn = "WFProfileName";
                sqa.InsertCommand.Parameters.Add("@WFTimeName", SqlDbType.NVarChar, 100).SourceColumn = "WFTimeName";
                sqa.InsertCommand.Parameters.Add("@ActID", SqlDbType.Int).SourceColumn = "ActID";
                sqa.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.InsertCommand.Parameters.Add("@ApprLevelID", SqlDbType.UniqueIdentifier).SourceColumn = "ApprLevelID";
                sqa.InsertCommand.Parameters.Add("@RegID1", SqlDbType.Int).SourceColumn = "RegID1";
                sqa.InsertCommand.Parameters.Add("@RegID2", SqlDbType.Int).SourceColumn = "RegID2";
                sqa.InsertCommand.Parameters.Add("@RegID", SqlDbType.NVarChar, 100).SourceColumn = "RegID";
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 20; i++)
                {
                    string parameterName = $"@Attr{i}";
                    string sourceColumnName = $"Attr{i}";
                    sqa.InsertCommand.Parameters.Add(parameterName, SqlDbType.NVarChar, 100).SourceColumn = sourceColumnName;
                }

                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 12; i++)
                {
                    string parameterName = $"@AttrVal{i}";
                    string sourceColumnName = $"AttrVal{i}";
                    sqa.InsertCommand.Parameters.Add(parameterName, SqlDbType.Decimal).SourceColumn = sourceColumnName;
                }
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 5; i++)
                {
                    string parameterName = $"@DateVal{i}";
                    string sourceColumnName = $"DateVal{i}";
                    sqa.InsertCommand.Parameters.Add(parameterName, SqlDbType.DateTime).SourceColumn = sourceColumnName;
                }

                // Add parameters for additional columns
                sqa.InsertCommand.Parameters.Add("@SpreadAmount", SqlDbType.Decimal).SourceColumn = "SpreadAmount";
                sqa.InsertCommand.Parameters.Add("@SpreadCurve", SqlDbType.NVarChar, 20).SourceColumn = "SpreadCurve";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 100).SourceColumn = "Status";
				sqa.InsertCommand.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid";

                sqa.InsertCommand.Parameters.Add("@CreateDate", SqlDbType.DateTime).SourceColumn = "CreateDate";
                sqa.InsertCommand.Parameters.Add("@CreateUser", SqlDbType.NVarChar, 50).SourceColumn = "CreateUser";
                sqa.InsertCommand.Parameters.Add("@UpdateDate", SqlDbType.DateTime).SourceColumn = "UpdateDate";
                sqa.InsertCommand.Parameters.Add("@UpdateUser", SqlDbType.NVarChar, 50).SourceColumn = "UpdateUser";

                var updateQuery = @"UPDATE RegPlan SET
                        Entity = @Entity, ApprLevelID = @ApprLevelID, 
                        RegID1 = @RegID1, RegID2 = @RegID2, RegID = @RegID, Attr1 = @Attr1, 
                        Attr2 = @Attr2, Attr3 = @Attr3, Attr4 = @Attr4, Attr5 = @Attr5, 
						Attr6 = @Attr6,Attr7 = @Attr7,Attr8 = @Attr8,Attr9 = @Attr9,
    					Attr10 = @Attr10,Attr11 = @Attr11,Attr12 = @Attr12,Attr13 = @Attr13,
    					Attr14 = @Attr14,Attr15 = @Attr15,Attr16 = @Attr16,Attr17 = @Attr17,
    					Attr18 = @Attr18,Attr19 = @Attr19,Attr20 = @Attr20,AttrVal1 = @AttrVal1,
    					AttrVal2 = @AttrVal2,AttrVal3 = @AttrVal3,AttrVal4 = @AttrVal4,
                        AttrVal5 = @AttrVal5,AttrVal6 = @AttrVal6,AttrVal7 = @AttrVal7,
  						AttrVal8 = @AttrVal8,AttrVal9 = @AttrVal9,AttrVal10 = @AttrVal10,
    					AttrVal11 = @AttrVal11,AttrVal12 = @AttrVal12,DateVal1 = @DateVal1,
    					DateVal2 = @DateVal2,DateVal3 = @DateVal3,DateVal4 = @DateVal4,DateVal5 = @DateVal5,
    					SpreadAmount = @SpreadAmount,SpreadCurve = @SpreadCurve,Status = @Status,Invalid = @Invalid,
                        UpdateDate = @UpdateDate, UpdateUser = @UpdateUser
                    WHERE RegPlanID = @RegPlanID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@RegPlanID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlanID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.UpdateCommand.Parameters.Add("@ApprLevelID", SqlDbType.UniqueIdentifier).SourceColumn = "ApprLevelID";
                sqa.UpdateCommand.Parameters.Add("@RegID1", SqlDbType.Int).SourceColumn = "RegID1";
                sqa.UpdateCommand.Parameters.Add("@RegID2", SqlDbType.Int).SourceColumn = "RegID2";
                sqa.UpdateCommand.Parameters.Add("@RegID", SqlDbType.NVarChar, 100).SourceColumn = "RegID";
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 20; i++)
                {
                    string parameterName = $"@Attr{i}";
                    string sourceColumnName = $"Attr{i}";
                    sqa.UpdateCommand.Parameters.Add(parameterName, SqlDbType.NVarChar, 100).SourceColumn = sourceColumnName;
                }

                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 12; i++)
                {
                    string parameterName = $"@AttrVal{i}";
                    string sourceColumnName = $"AttrVal{i}";
                    sqa.UpdateCommand.Parameters.Add(parameterName, SqlDbType.Decimal).SourceColumn = sourceColumnName;
                }
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 5; i++)
                {
                    string parameterName = $"@DateVal{i}";
                    string sourceColumnName = $"DateVal{i}";
                    sqa.UpdateCommand.Parameters.Add(parameterName, SqlDbType.DateTime).SourceColumn = sourceColumnName;
                }

                // Add parameters for additional columns
                sqa.UpdateCommand.Parameters.Add("@SpreadAmount", SqlDbType.Decimal).SourceColumn = "SpreadAmount";
                sqa.UpdateCommand.Parameters.Add("@SpreadCurve", SqlDbType.NVarChar, 20).SourceColumn = "SpreadCurve";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 100).SourceColumn = "Status";

                sqa.UpdateCommand.Parameters.Add("@CreateDate", SqlDbType.DateTime).SourceColumn = "CreateDate";
                sqa.UpdateCommand.Parameters.Add("@CreateUser", SqlDbType.NVarChar, 50).SourceColumn = "CreateUser";
                sqa.UpdateCommand.Parameters.Add("@UpdateDate", SqlDbType.DateTime).SourceColumn = "UpdateDate";
                sqa.UpdateCommand.Parameters.Add("@UpdateUser", SqlDbType.NVarChar, 50).SourceColumn = "UpdateUser";
                sqa.UpdateCommand.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid";

                string deleteQuery = "DELETE FROM RegPlan WHERE RegPlanID = @RegPlanID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@RegPlanID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlanID", SourceVersion = DataRowVersion.Original });

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