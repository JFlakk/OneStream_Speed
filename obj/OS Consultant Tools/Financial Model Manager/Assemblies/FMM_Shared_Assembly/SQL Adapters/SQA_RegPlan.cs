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
    public class SQA_RegPlan
    {
        private readonly SqlConnection _connection;

        public SQA_RegPlan(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_RegPlan_DataTable(SessionInfo si, SqlDataAdapter adapter, DataTable dataTable, string selectQuery, params SqlParameter[] parameters)
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

        public void Update_RegPlan(SessionInfo si, DataTable dataTable, SqlDataAdapter adapter)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                string insertQuery = @"
                    INSERT INTO RegPlan (
                        RegPlan_ID, WF_Scenario_Name, WF_Profile_Name, WF_Time_Name, Act_ID, Entity, Approval_Level_ID, 
                        Register_ID_1, Register_ID_2, Register_ID, Attribute_1, Attribute_2, Attribute_3, Attribute_4, Attribute_5, 
                        Attribute_6, Attribute_7, Attribute_8, Attribute_9, Attribute_10, Attribute_11, Attribute_12, Attribute_13, Attribute_14, 
                        Attribute_15, Attribute_16, Attribute_17, Attribute_18, Attribute_19, Attribute_20, Attribute_Value_1, Attribute_Value_2, 
                        Attribute_Value_3, Attribute_Value_4, Attribute_Value_5, Attribute_Value_6, Attribute_Value_7, Attribute_Value_8, Attribute_Value_9, 
                        Attribute_Value_10, Attribute_Value_11, Attribute_Value_12, Date_Value_1, Date_Value_2, Date_Value_3, Date_Value_4, Date_Value_5, 
                        Spread_Amount, Spread_Curve, Status, Create_Date, Create_User, Update_Date, Update_User, Invalid
                    ) VALUES (
                        @RegPlan_ID, @WF_Scenario_Name, @WF_Profile_Name, @WF_Time_Name, @Act_ID,@Entity, @Approval_Level_ID, 
                        @Register_ID_1, @Register_ID_2, @Register_ID, @Attribute_1, @Attribute_2, @Attribute_3, @Attribute_4, @Attribute_5, 
                        @Attribute_6, @Attribute_7, @Attribute_8, @Attribute_9, @Attribute_10, @Attribute_11, @Attribute_12, @Attribute_13, @Attribute_14, 
                        @Attribute_15, @Attribute_16, @Attribute_17, @Attribute_18, @Attribute_19, @Attribute_20, @Attribute_Value_1, @Attribute_Value_2, 
                        @Attribute_Value_3, @Attribute_Value_4, @Attribute_Value_5, @Attribute_Value_6, @Attribute_Value_7, @Attribute_Value_8, @Attribute_Value_9, 
                        @Attribute_Value_10, @Attribute_Value_11, @Attribute_Value_12, @Date_Value_1, @Date_Value_2, @Date_Value_3, @Date_Value_4, @Date_Value_5, 
                        @Spread_Amount, @Spread_Curve, @Status, @Create_Date, @Create_User, @Update_Date, @Update_User, @Invalid
                    )";

                adapter.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                adapter.InsertCommand.Parameters.Add("@RegPlan_ID", SqlDbType.UniqueIdentifier).SourceColumn = "RegPlan_ID";
                adapter.InsertCommand.Parameters.Add("@WF_Scenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WF_Scenario_Name";
                adapter.InsertCommand.Parameters.Add("@WF_Profile_Name", SqlDbType.NVarChar, 100).SourceColumn = "WF_Profile_Name";
                adapter.InsertCommand.Parameters.Add("@WF_Time_Name", SqlDbType.NVarChar, 100).SourceColumn = "WF_Time_Name";
                adapter.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                adapter.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                adapter.InsertCommand.Parameters.Add("@Approval_Level_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Approval_Level_ID";
                adapter.InsertCommand.Parameters.Add("@Register_ID_1", SqlDbType.Int).SourceColumn = "Register_ID_1";
                adapter.InsertCommand.Parameters.Add("@Register_ID_2", SqlDbType.Int).SourceColumn = "Register_ID_2";
                adapter.InsertCommand.Parameters.Add("@Register_ID", SqlDbType.NVarChar, 100).SourceColumn = "Register_ID";
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 20; i++)
                {
                    string parameterName = $"@Attribute_{i}";
                    string sourceColumnName = $"Attribute_{i}";
                    adapter.InsertCommand.Parameters.Add(parameterName, SqlDbType.NVarChar, 100).SourceColumn = sourceColumnName;
                }

                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 12; i++)
                {
                    string parameterName = $"@Attribute_Value_{i}";
                    string sourceColumnName = $"Attribute_Value_{i}";
                    adapter.InsertCommand.Parameters.Add(parameterName, SqlDbType.Decimal).SourceColumn = sourceColumnName;
                }
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 5; i++)
                {
                    string parameterName = $"@Date_Value_{i}";
                    string sourceColumnName = $"Date_Value_{i}";
                    adapter.InsertCommand.Parameters.Add(parameterName, SqlDbType.DateTime).SourceColumn = sourceColumnName;
                }

                // Add parameters for additional columns
                adapter.InsertCommand.Parameters.Add("@Spread_Amount", SqlDbType.Decimal).SourceColumn = "Spread_Amount";
                adapter.InsertCommand.Parameters.Add("@Spread_Curve", SqlDbType.NVarChar, 20).SourceColumn = "Spread_Curve";
                adapter.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 100).SourceColumn = "Status";

                adapter.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                adapter.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                adapter.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                adapter.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
                adapter.InsertCommand.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid";



                string updateQuery = @"
                    UPDATE RegPlan SET
                        Entity = @Entity, Approval_Level_ID = @Approval_Level_ID, 
                        Register_ID_1 = @Register_ID_1, Register_ID_2 = @Register_ID_2, Register_ID = @Register_ID, Attribute_1 = @Attribute_1, 
                        Attribute_2 = @Attribute_2, Attribute_3 = @Attribute_3, Attribute_4 = @Attribute_4, Attribute_5 = @Attribute_5, 
						Attribute_6 = @Attribute_6,Attribute_7 = @Attribute_7,Attribute_8 = @Attribute_8,Attribute_9 = @Attribute_9,
    					Attribute_10 = @Attribute_10,Attribute_11 = @Attribute_11,Attribute_12 = @Attribute_12,Attribute_13 = @Attribute_13,
    					Attribute_14 = @Attribute_14,Attribute_15 = @Attribute_15,Attribute_16 = @Attribute_16,Attribute_17 = @Attribute_17,
    					Attribute_18 = @Attribute_18,Attribute_19 = @Attribute_19,Attribute_20 = @Attribute_20,Attribute_Value_1 = @Attribute_Value_1,
    					Attribute_Value_2 = @Attribute_Value_2,Attribute_Value_3 = @Attribute_Value_3,Attribute_Value_4 = @Attribute_Value_4,
                        Attribute_Value_5 = @Attribute_Value_5,Attribute_Value_6 = @Attribute_Value_6,Attribute_Value_7 = @Attribute_Value_7,
  						Attribute_Value_8 = @Attribute_Value_8,Attribute_Value_9 = @Attribute_Value_9,Attribute_Value_10 = @Attribute_Value_10,
    					Attribute_Value_11 = @Attribute_Value_11,Attribute_Value_12 = @Attribute_Value_12,Date_Value_1 = @Date_Value_1,
    					Date_Value_2 = @Date_Value_2,Date_Value_3 = @Date_Value_3,Date_Value_4 = @Date_Value_4,Date_Value_5 = @Date_Value_5,
    					Spread_Amount = @Spread_Amount,Spread_Curve = @Spread_Curve,Status = @Status,Invalid = @Invalid,
                        Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE RegPlan_ID = @RegPlan_ID";

                adapter.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                adapter.UpdateCommand.Parameters.Add(new SqlParameter("@RegPlan_ID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlan_ID", SourceVersion = DataRowVersion.Original });
                adapter.UpdateCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                adapter.UpdateCommand.Parameters.Add("@Approval_Level_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Approval_Level_ID";
                adapter.UpdateCommand.Parameters.Add("@Register_ID_1", SqlDbType.Int).SourceColumn = "Register_ID_1";
                adapter.UpdateCommand.Parameters.Add("@Register_ID_2", SqlDbType.Int).SourceColumn = "Register_ID_2";
                adapter.UpdateCommand.Parameters.Add("@Register_ID", SqlDbType.NVarChar, 100).SourceColumn = "Register_ID";
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 20; i++)
                {
                    string parameterName = $"@Attribute_{i}";
                    string sourceColumnName = $"Attribute_{i}";
                    adapter.UpdateCommand.Parameters.Add(parameterName, SqlDbType.NVarChar, 100).SourceColumn = sourceColumnName;
                }

                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 12; i++)
                {
                    string parameterName = $"@Attribute_Value_{i}";
                    string sourceColumnName = $"Attribute_Value_{i}";
                    adapter.UpdateCommand.Parameters.Add(parameterName, SqlDbType.Decimal).SourceColumn = sourceColumnName;
                }
                // Add parameters for Attributes dynamically
                for (int i = 1; i <= 5; i++)
                {
                    string parameterName = $"@Date_Value_{i}";
                    string sourceColumnName = $"Date_Value_{i}";
                    adapter.UpdateCommand.Parameters.Add(parameterName, SqlDbType.DateTime).SourceColumn = sourceColumnName;
                }

                // Add parameters for additional columns
                adapter.UpdateCommand.Parameters.Add("@Spread_Amount", SqlDbType.Decimal).SourceColumn = "Spread_Amount";
                adapter.UpdateCommand.Parameters.Add("@Spread_Curve", SqlDbType.NVarChar, 20).SourceColumn = "Spread_Curve";
                adapter.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 100).SourceColumn = "Status";

                adapter.UpdateCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                adapter.UpdateCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                adapter.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                adapter.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
                adapter.UpdateCommand.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid";

                string deleteQuery = "DELETE FROM RegPlan WHERE RegPlan_ID = @RegPlan_ID";
                adapter.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@RegPlan_ID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlan_ID", SourceVersion = DataRowVersion.Original });

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