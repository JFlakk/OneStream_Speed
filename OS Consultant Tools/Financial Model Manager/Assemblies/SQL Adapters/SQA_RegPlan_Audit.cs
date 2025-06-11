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
    public class SQA_Reg_Plan_Audit
    {
        private readonly SqlConnection _connection;

        public SQA_Reg_Plan_Audit(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void Fill_RegPlan_Audit_DataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dataTable, string selectQuery, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dataTable);
            }
        }

        public void Update_RegPlan_Audit(SessionInfo si, DataTable dataTable, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {

                // Define the insert query and parameters
                string insertQuery = @"
				    INSERT INTO [dbo].[RegPlan_Audit] (
				        [RegPlan_ID], [WF_Scenario_Name], [Project_ID], [Entity], 
				        [Resource_ID], [Spread_Amount], [Spread_Curve], 
				        [Attribute_1], [Attribute_2], [Attribute_3], [Attribute_4], 
				        [Attribute_5], [Attribute_6], [Attribute_7], [Attribute_8], 
				        [Attribute_9], [Attribute_10], [Attribute_11], [Attribute_12], 
				        [Attribute_13], [Attribute_14], [Attribute_15], [Attribute_16], 
				        [Attribute_17], [Attribute_18], [Attribute_19], [Attribute_20], 
						[Attribute_Value_1],[Attribute_Value_2],[Attribute_Value_3],
						[Attribute_Value_4],[Attribute_Value_5],[Attribute_Value_6],
						[Attribute_Value_7],[Attribute_Value_8],[Attribute_Value_9],
						[Attribute_Value_10],[Attribute_Value_11],[Attribute_Value_12],
						[Date_Value_1],[Date_Value_2],[Date_Value_3],
						[Date_Value_4],[Date_Value_5]
					   ) VALUES (
					     @RegPlan_ID, @WF_Scenario_Name, @Project_ID, @Entity, 
					     @Resource_ID,@Spread_Amount, @Spread_Curve, 
					     @Attribute_1, @Attribute_2, @Attribute_3, @Attribute_4, 
					     @Attribute_5, @Attribute_6, @Attribute_7, @Attribute_8, 
						 @Attribute_9, @Attribute_10, @Attribute11, @Attribute_12, 
						 @Attribute_13, @Attribute_14, @Attribute_15, @Attribute_16, 
						 @Attribute_17, @Attribute_18, @Attribute_19, @Attribute_20,
						 @Attribute_Value_1,@Attribute_Value_2,@Attribute_Value_3,
						 @Attribute_Value_4,@Attribute_Value_5,@Attribute_Value_6,
						 @Attribute_Value_7,@Attribute_Value_8,@Attribute_Value_9,
						 @Attribute_Value_10,@Attribute_Value_11,@Attribute_Value_12,
						 @Date_Value_1,@Date_Value_2,@Date_Value_3,
						 @Date_Value_4,@Date_Value_5);";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@RegPlan_ID", SqlDbType.UniqueIdentifier).SourceColumn = "RegPlan_ID";
                sqa.InsertCommand.Parameters.Add("@WF_Scenario_Name", SqlDbType.NVarChar).SourceColumn = "WF_Scenario_Name";
                sqa.InsertCommand.Parameters.Add("@Project_ID", SqlDbType.VarChar).SourceColumn = "Project_ID";
                sqa.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar).SourceColumn = "Entity";
                sqa.InsertCommand.Parameters.Add("@Resource_ID", SqlDbType.NVarChar).SourceColumn = "Resource_ID";
                sqa.InsertCommand.Parameters.Add("@Spread_Amount", SqlDbType.Decimal).SourceColumn = "Spread_Amount";
                sqa.InsertCommand.Parameters.Add("@Spread_Curve", SqlDbType.NVarChar).SourceColumn = "Spread_Curve";
                // Add parameters for Attribute_1 through Attribute_20
                for (int i = 1; i <= 20; i++)
                {
                    sqa.InsertCommand.Parameters.Add("@Attribute_{i}", SqlDbType.NVarChar).SourceColumn = "Attribute_{i}";
                }
                // Add parameters for Attribute_Value_1 through Attribute_Value_12
                for (int i = 1; i <= 12; i++)
                {
                    sqa.InsertCommand.Parameters.Add("@Attribute_Value_{i}", SqlDbType.Decimal).SourceColumn = "Attribute_Value_{i}";
                }
                // Add parameters for Date_Value_1 through Date_Value_5
                for (int i = 1; i <= 5; i++)
                {
                    sqa.InsertCommand.Parameters.Add("@Date_Value_{i}", SqlDbType.DateTime).SourceColumn = "Date_Value_{i}";
                }

                // Define the update query and parameters
                string updateQuery = @"
				    UPDATE [dbo].[RegPlan] SET
				        [WF_Scenario_Name] = @WF_Scenario_Name, 
				        [Project_ID] = @Project_ID, [Entity] = @Entity, 
				        [Resource_ID] = @Resource_ID, [Spread_Amount] = @Spread_Amount, 
						[Spread_Curve] = @Spread_Curve, 
				        [Attribute_1] = @Attribute_1, [Attribute_2] = @Attribute_2, 
				        [Attribute_3] = @Attribute_3, [Attribute_4] = @Attribute_4, 
				        [Attribute_5] = @Attribute_5, [Attribute_6] = @Attribute_6, 
				        [Attribute_7] = @Attribute_7, [Attribute_8] = @Attribute_8, 
				        [Attribute_9] = @Attribute_9, [Attribute_10] = @Attribute_10, 
				        [Attribute_11] = @Attribute_11, [Attribute_12] = @Attribute_12, 
				        [Attribute_13] = @Attribute_13, [Attribute_14] = @Attribute_14, 
				        [Attribute_15] = @Attribute_15, [Attribute_16] = @Attribute_16, 
				        [Attribute_17] = @Attribute_17, [Attribute_18] = @Attribute_18, 
				        [Attribute_19] = @Attribute_19, [Attribute_20] = @Attribute_20, 
				        [Attribute_Value_1] = @Attribute_Value_1, [Attribute_Value_2] = @Attribute_Value_2, 
				        [Attribute_Value_3] = @Attribute_Value_3, [Attribute_Value_4] = @Attribute_Value_4, 
				        [Attribute_Value_5] = @Attribute_Value_5, [Attribute_Value_6] = @Attribute_Value_6, 
				        [Attribute_Value_7] = @Attribute_Value_7, [Attribute_Value_8] = @Attribute_Value_8, 
				        [Attribute_Value_9] = @Attribute_Value_9, [Attribute_Value_10] = @Attribute_Value_10, 
				        [Attribute_Value_11] = @Attribute_Value_11, [Attribute_Value_12] = @Attribute_Value_12,
						[Date_Value_1] = @Date_Value_1, [Date_Value_2] = @Date_Value_2,
						[Date_Value_3] = @Date_Value_3,[Date_Value_4] = @Date_Value_4,
						[Date_Value_5] = @Date_Value_5
				    WHERE [RegPlan_ID] = @RegPlan_ID
					AND [WF_Scenario_Name] = @WF_Scenario_Name
					AND [Project_ID] = @Project_ID
					AND [Entity] = @Entity";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add("@RegPlan_ID", SqlDbType.UniqueIdentifier).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@WF_Scenario_Name", SqlDbType.NVarChar).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@Project_ID", SqlDbType.VarChar).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@Entity", SqlDbType.NVarChar).SourceVersion = DataRowVersion.Original;
                sqa.UpdateCommand.Parameters.Add("@Resource_ID", SqlDbType.NVarChar).SourceColumn = "Resource_ID";
                sqa.UpdateCommand.Parameters.Add("@Spread_Amount", SqlDbType.Decimal).SourceColumn = "Spread_Amount";
                sqa.UpdateCommand.Parameters.Add("@Spread_Curve", SqlDbType.NVarChar).SourceColumn = "Spread_Curve";
                // Add parameters for Attribute_1 through Attribute_20
                for (int i = 1; i <= 20; i++)
                {
                    sqa.UpdateCommand.Parameters.Add("@Attribute_{i}", SqlDbType.NVarChar);
                }
                // Add parameters for Attribute_Value_1 through Attribute_Value_12
                for (int i = 1; i <= 12; i++)
                {
                    sqa.UpdateCommand.Parameters.Add("@Attribute_Value_{i}", SqlDbType.Decimal);
                }

                // Add parameters for Date_Value_1 through Date_Value_5
                for (int i = 1; i <= 5; i++)
                {
                    sqa.UpdateCommand.Parameters.Add("@Date_Value_{i}", SqlDbType.DateTime);
                }

                // Define the delete query and parameters
                string deleteQuery = @"
							DELETE FROM [dbo].[RegPlan]
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
                    sqa.Update(dataTable);
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