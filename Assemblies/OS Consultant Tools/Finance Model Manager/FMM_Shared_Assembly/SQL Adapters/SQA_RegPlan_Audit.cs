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
    public class SQA_RegPlan_Audit
    {
        private readonly SqlConnection _connection;

        public SQA_RegPlan_Audit(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void Fill_RegPlan_Audit_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_RegPlan_Audit(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {

                // Define the insert query and parameters
                string insertQuery = @"
				    INSERT INTO [dbo].[RegPlan_Audit] (
				        [RegisterPlanID], [WFScenarioName], [ProjectID], [Entity], 
				        [ResourceID], [SpreadAmount], [SpreadCurve], 
				        [Attribute1], [Attribute2], [Attribute3], [Attribute4], 
				        [Attribute5], [Attribute6], [Attribute7], [Attribute8], 
				        [Attribute9], [Attribute10], [Attribute11], [Attribute12], 
				        [Attribute13], [Attribute14], [Attribute15], [Attribute16], 
				        [Attribute17], [Attribute18], [Attribute19], [Attribute20], 
						[AttributeValue1],[AttributeValue2],[AttributeValue3],
						[AttributeValue4],[AttributeValue5],[AttributeValue6],
						[AttributeValue7],[AttributeValue8],[AttributeValue9],
						[AttributeValue10],[AttributeValue11],[AttributeValue12],
						[DateValue1],[DateValue2],[DateValue3],
						[DateValue4],[DateValue5]
					   ) VALUES (
					     @RegisterPlanID, @WFScenarioName, @ProjectID, @Entity, 
					     @ResourceID,@SpreadAmount, @SpreadCurve, 
					     @Attribute1, @Attribute2, @Attribute3, @Attribute4, 
					     @Attribute5, @Attribute6, @Attribute7, @Attribute8, 
						 @Attribute9, @Attribute10, @Attribute11, @Attribute12, 
						 @Attribute13, @Attribute14, @Attribute15, @Attribute16, 
						 @Attribute17, @Attribute18, @Attribute19, @Attribute20,
						 @AttributeValue1,@AttributeValue2,@AttributeValue3,
						 @AttributeValue4,@AttributeValue5,@AttributeValue6,
						 @AttributeValue7,@AttributeValue8,@AttributeValue9,
						 @AttributeValue10,@AttributeValue11,@AttributeValue12,
						 @DateValue1,@DateValue2,@DateValue3,
						 @DateValue4,@DateValue5);";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@RegisterPlanID", SqlDbType.UniqueIdentifier).SourceColumn = "RegisterPlanID";
                sqa.InsertCommand.Parameters.Add("@WFScenarioName", SqlDbType.NVarChar).SourceColumn = "WFScenarioName";
                sqa.InsertCommand.Parameters.Add("@ProjectID", SqlDbType.VarChar).SourceColumn = "ProjectID";
                sqa.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar).SourceColumn = "Entity";
                sqa.InsertCommand.Parameters.Add("@ResourceID", SqlDbType.NVarChar).SourceColumn = "ResourceID";
                sqa.InsertCommand.Parameters.Add("@SpreadAmount", SqlDbType.Decimal).SourceColumn = "SpreadAmount";
                sqa.InsertCommand.Parameters.Add("@SpreadCurve", SqlDbType.NVarChar).SourceColumn = "SpreadCurve";
                // Add parameters for Attribute1 through Attribute20
                for (int i = 1; i <= 20; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@Attribute{i}", SqlDbType.NVarChar).SourceColumn = $"Attribute{i}";
                }
                // Add parameters for AttributeValue1 through AttributeValue12
                for (int i = 1; i <= 12; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@AttributeValue{i}", SqlDbType.Decimal).SourceColumn = $"AttributeValue{i}";
                }
                // Add parameters for DateValue1 through DateValue5
                for (int i = 1; i <= 5; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@DateValue{i}", SqlDbType.DateTime).SourceColumn = $"DateValue{i}";
                }

                // Define the update query and parameters
                string updateQuery = @"
				    UPDATE [dbo].[RegisterPlan] SET
				        [WFScenarioName] = @WFScenarioName, 
				        [ProjectID] = @ProjectID, [Entity] = @Entity, 
				        [ResourceID] = @ResourceID, [SpreadAmount] = @SpreadAmount, 
						[SpreadCurve] = @SpreadCurve, 
				        [Attribute1] = @Attribute1, [Attribute2] = @Attribute2, 
				        [Attribute3] = @Attribute3, [Attribute4] = @Attribute4, 
				        [Attribute5] = @Attribute5, [Attribute6] = @Attribute6, 
				        [Attribute7] = @Attribute7, [Attribute8] = @Attribute8, 
				        [Attribute9] = @Attribute9, [Attribute10] = @Attribute10, 
				        [Attribute11] = @Attribute11, [Attribute12] = @Attribute12, 
				        [Attribute13] = @Attribute13, [Attribute14] = @Attribute14, 
				        [Attribute15] = @Attribute15, [Attribute16] = @Attribute16, 
				        [Attribute17] = @Attribute17, [Attribute18] = @Attribute18, 
				        [Attribute19] = @Attribute19, [Attribute20] = @Attribute20, 
				        [AttributeValue1] = @AttributeValue1, [AttributeValue2] = @AttributeValue2, 
				        [AttributeValue3] = @AttributeValue3, [AttributeValue4] = @AttributeValue4, 
				        [AttributeValue5] = @AttributeValue5, [AttributeValue6] = @AttributeValue6, 
				        [AttributeValue7] = @AttributeValue7, [AttributeValue8] = @AttributeValue8, 
				        [AttributeValue9] = @AttributeValue9, [AttributeValue10] = @AttributeValue10, 
				        [AttributeValue11] = @AttributeValue11, [AttributeValue12] = @AttributeValue12,
						[DateValue1] = @DateValue1, [DateValue2] = @DateValue2,
						[DateValue3] = @DateValue3,[DateValue4] = @DateValue4,
						[DateValue5] = @DateValue5
				    WHERE [RegisterPlanID] = @RegisterPlanID
					AND [WFScenarioName] = @WFScenarioName
					AND [ProjectID] = @ProjectID
					AND [Entity] = @Entity";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add("@RegisterPlanID", SqlDbType.UniqueIdentifier).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@WFScenarioName", SqlDbType.NVarChar).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@ProjectID", SqlDbType.VarChar).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@Entity", SqlDbType.NVarChar).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@ResourceID", SqlDbType.NVarChar).SourceColumn = "ResourceID";
                sqa.UpdateCommand.Parameters.Add("@SpreadAmount", SqlDbType.Decimal).SourceColumn = "SpreadAmount";
                sqa.UpdateCommand.Parameters.Add("@SpreadCurve", SqlDbType.NVarChar).SourceColumn = "SpreadCurve";
                // Add parameters for Attribute1 through Attribute20
                for (int i = 1; i <= 20; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@Attribute{i}", SqlDbType.NVarChar);
                }
                // Add parameters for AttributeValue1 through AttributeValue12
                for (int i = 1; i <= 12; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@AttributeValue{i}", SqlDbType.Decimal);
                }

                // Add parameters for DateValue1 through DateValue5
                for (int i = 1; i <= 5; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@DateValue{i}", SqlDbType.DateTime);
                }

                // Define the delete query and parameters
                string deleteQuery = @"
							DELETE FROM [dbo].[RegisterPlan]
							WHERE RegisterID = @RegisterID AND RegisterIDInstance = @RegisterIDInstance
						    AND WFProfileName = @WFProfileName AND WFScenarioName = @WFScenarioName
						    AND WFTimeName = @WFTimeName";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add("@RegisterID", SqlDbType.VarChar);
                sqa.DeleteCommand.Parameters.Add("@RegisterIDInstance", SqlDbType.Int);
                sqa.DeleteCommand.Parameters.Add("@WFProfileName", SqlDbType.VarChar);
                sqa.DeleteCommand.Parameters.Add("@WFScenarioName", SqlDbType.VarChar);
                sqa.DeleteCommand.Parameters.Add("@WFTimeName", SqlDbType.VarChar);

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