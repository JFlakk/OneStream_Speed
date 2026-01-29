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
                        RegPlan_ID, WFScenario_Name, WFProfile_Name, WFTime_Name, ActID, Entity, Appr_Level_ID, 
                        Reg_ID_1, Reg_ID_2, Reg_ID, Attr_1, Attr_2, Attr_3, Attr_4, Attr_5, 
                        Attr_6, Attr_7, Attr_8, Attr_9, Attr_10, Attr_11, Attr_12, Attr_13, Attr_14, 
                        Attr_15, Attr_16, Attr_17, Attr_18, Attr_19, Attr_20, Attr_Val_1, Attr_Val_2, 
                        Attr_Val_3, Attr_Val_4, Attr_Val_5, Attr_Val_6, Attr_Val_7, Attr_Val_8, Attr_Val_9, 
                        Attr_Val_10, Attr_Val_11, Attr_Val_12, Date_Val_1, Date_Val_2, Date_Val_3, Date_Val_4, Date_Val_5, 
                        Spread_Amount, Spread_Curve, Status, Invalid, Create_Date, Create_User, Update_Date, Update_User
                    ) VALUES (
                        @RegPlan_ID, @WFScenario_Name, @WFProfile_Name, @WFTime_Name, @ActID,@Entity, @Appr_Level_ID, 
                        @Reg_ID_1, @Reg_ID_2, @Reg_ID, @Attr_1, @Attr_2, @Attr_3, @Attr_4, @Attr_5, 
                        @Attr_6, @Attr_7, @Attr_8, @Attr_9, @Attr_10, @Attr_11, @Attr_12, @Attr_13, @Attr_14, 
                        @Attr_15, @Attr_16, @Attr_17, @Attr_18, @Attr_19, @Attr_20, @Attr_Val_1, @Attr_Val_2, 
                        @Attr_Val_3, @Attr_Val_4, @Attr_Val_5, @Attr_Val_6, @Attr_Val_7, @Attr_Val_8, @Attr_Val_9, 
                        @Attr_Val_10, @Attr_Val_11, @Attr_Val_12, @Date_Val_1, @Date_Val_2, @Date_Val_3, @Date_Val_4, @Date_Val_5, 
                        @Spread_Amount, @Spread_Curve, @Status, @Invalid, @Create_Date, @Create_User, @Update_Date, @Update_User)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@RegPlan_ID", SqlDbType.UniqueIdentifier).SourceColumn = "RegPlan_ID";
                sqa.InsertCommand.Parameters.Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name";
                sqa.InsertCommand.Parameters.Add("@WFProfile_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFProfile_Name";
                sqa.InsertCommand.Parameters.Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name";
                sqa.InsertCommand.Parameters.Add("@ActID", SqlDbType.Int).SourceColumn = "ActID";
                sqa.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.InsertCommand.Parameters.Add("@Appr_Level_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Appr_Level_ID";
                sqa.InsertCommand.Parameters.Add("@Reg_ID_1", SqlDbType.Int).SourceColumn = "Reg_ID_1";
                sqa.InsertCommand.Parameters.Add("@Reg_ID_2", SqlDbType.Int).SourceColumn = "Reg_ID_2";
                sqa.InsertCommand.Parameters.Add("@Reg_ID", SqlDbType.NVarChar, 100).SourceColumn = "Reg_ID";
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 20; i++)
                {
                    string parameterName = $"@Attr_{i}";
                    string sourceColumnName = $"Attr_{i}";
                    sqa.InsertCommand.Parameters.Add(parameterName, SqlDbType.NVarChar, 100).SourceColumn = sourceColumnName;
                }

                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 12; i++)
                {
                    string parameterName = $"@Attr_Val_{i}";
                    string sourceColumnName = $"Attr_Val_{i}";
                    sqa.InsertCommand.Parameters.Add(parameterName, SqlDbType.Decimal).SourceColumn = sourceColumnName;
                }
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 5; i++)
                {
                    string parameterName = $"@Date_Val_{i}";
                    string sourceColumnName = $"Date_Val_{i}";
                    sqa.InsertCommand.Parameters.Add(parameterName, SqlDbType.DateTime).SourceColumn = sourceColumnName;
                }

                // Add parameters for additional columns
                sqa.InsertCommand.Parameters.Add("@Spread_Amount", SqlDbType.Decimal).SourceColumn = "Spread_Amount";
                sqa.InsertCommand.Parameters.Add("@Spread_Curve", SqlDbType.NVarChar, 20).SourceColumn = "Spread_Curve";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 100).SourceColumn = "Status";
				sqa.InsertCommand.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid";

                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                var updateQuery = @"UPDATE RegPlan SET
                        Entity = @Entity, Appr_Level_ID = @Appr_Level_ID, 
                        Reg_ID_1 = @Reg_ID_1, Reg_ID_2 = @Reg_ID_2, Reg_ID = @Reg_ID, Attr_1 = @Attr_1, 
                        Attr_2 = @Attr_2, Attr_3 = @Attr_3, Attr_4 = @Attr_4, Attr_5 = @Attr_5, 
						Attr_6 = @Attr_6,Attr_7 = @Attr_7,Attr_8 = @Attr_8,Attr_9 = @Attr_9,
    					Attr_10 = @Attr_10,Attr_11 = @Attr_11,Attr_12 = @Attr_12,Attr_13 = @Attr_13,
    					Attr_14 = @Attr_14,Attr_15 = @Attr_15,Attr_16 = @Attr_16,Attr_17 = @Attr_17,
    					Attr_18 = @Attr_18,Attr_19 = @Attr_19,Attr_20 = @Attr_20,Attr_Val_1 = @Attr_Val_1,
    					Attr_Val_2 = @Attr_Val_2,Attr_Val_3 = @Attr_Val_3,Attr_Val_4 = @Attr_Val_4,
                        Attr_Val_5 = @Attr_Val_5,Attr_Val_6 = @Attr_Val_6,Attr_Val_7 = @Attr_Val_7,
  						Attr_Val_8 = @Attr_Val_8,Attr_Val_9 = @Attr_Val_9,Attr_Val_10 = @Attr_Val_10,
    					Attr_Val_11 = @Attr_Val_11,Attr_Val_12 = @Attr_Val_12,Date_Val_1 = @Date_Val_1,
    					Date_Val_2 = @Date_Val_2,Date_Val_3 = @Date_Val_3,Date_Val_4 = @Date_Val_4,Date_Val_5 = @Date_Val_5,
    					Spread_Amount = @Spread_Amount,Spread_Curve = @Spread_Curve,Status = @Status,Invalid = @Invalid,
                        Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE RegPlan_ID = @RegPlan_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@RegPlan_ID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlan_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.UpdateCommand.Parameters.Add("@Appr_Level_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Appr_Level_ID";
                sqa.UpdateCommand.Parameters.Add("@Reg_ID_1", SqlDbType.Int).SourceColumn = "Reg_ID_1";
                sqa.UpdateCommand.Parameters.Add("@Reg_ID_2", SqlDbType.Int).SourceColumn = "Reg_ID_2";
                sqa.UpdateCommand.Parameters.Add("@Reg_ID", SqlDbType.NVarChar, 100).SourceColumn = "Reg_ID";
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 20; i++)
                {
                    string parameterName = $"@Attr_{i}";
                    string sourceColumnName = $"Attr_{i}";
                    sqa.UpdateCommand.Parameters.Add(parameterName, SqlDbType.NVarChar, 100).SourceColumn = sourceColumnName;
                }

                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 12; i++)
                {
                    string parameterName = $"@Attr_Val_{i}";
                    string sourceColumnName = $"Attr_Val_{i}";
                    sqa.UpdateCommand.Parameters.Add(parameterName, SqlDbType.Decimal).SourceColumn = sourceColumnName;
                }
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 5; i++)
                {
                    string parameterName = $"@Date_Val_{i}";
                    string sourceColumnName = $"Date_Val_{i}";
                    sqa.UpdateCommand.Parameters.Add(parameterName, SqlDbType.DateTime).SourceColumn = sourceColumnName;
                }

                // Add parameters for additional columns
                sqa.UpdateCommand.Parameters.Add("@Spread_Amount", SqlDbType.Decimal).SourceColumn = "Spread_Amount";
                sqa.UpdateCommand.Parameters.Add("@Spread_Curve", SqlDbType.NVarChar, 20).SourceColumn = "Spread_Curve";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 100).SourceColumn = "Status";

                sqa.UpdateCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.UpdateCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
                sqa.UpdateCommand.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid";

                string deleteQuery = "DELETE FROM RegPlan WHERE RegPlan_ID = @RegPlan_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@RegPlan_ID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlan_ID", SourceVersion = DataRowVersion.Original });

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