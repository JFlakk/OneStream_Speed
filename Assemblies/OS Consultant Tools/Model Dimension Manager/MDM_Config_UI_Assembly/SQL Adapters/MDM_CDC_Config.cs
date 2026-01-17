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
    public class SQA_MDM_CDC_Config
    {
        private readonly SqlConnection _connection;

        public SQA_MDM_CDC_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_MDM_CDC_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
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

        public void Update_MDM_CDC_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "MDM_CDC_Config", dt);
                    builder.SetPrimaryKey("CDC_Config_ID");
                    builder.ExcludeFromUpdate("CDC_Config_ID", "Create_Date", "Create_User");
                    builder.ConfigureAdapter(sqa, transaction);

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

        public DataTable Get_CDC_Config_By_ID(SessionInfo si, int cdcConfigID)
        {
            try
            {
                var dt = new DataTable("CDC_Config");
                var sql = @"SELECT 
                                CDC_Config_ID,
                                DimTypeID,
                                DimID,
                                SourceType,
                                SourceConfig,
                                Map_Name,
                                Map_Description,
                                Map_Text1,
                                Map_Text2,
                                Map_Text3,
                                Map_Text4,
                                Map_Text5,
                                Map_Text6,
                                Map_Text7,
                                Map_Text8,
                                Create_Date,
                                Create_User,
                                Modify_Date,
                                Modify_User
                            FROM MDM_CDC_Config
                            WHERE CDC_Config_ID = @CDC_Config_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { Value = cdcConfigID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_CDC_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public DataTable Get_CDC_Config_By_Dimension(SessionInfo si, int dimTypeID, int dimID)
        {
            try
            {
                var dt = new DataTable("CDC_Config");
                var sql = @"SELECT 
                                CDC_Config_ID,
                                DimTypeID,
                                DimID,
                                SourceType,
                                SourceConfig,
                                Map_Name,
                                Map_Description,
                                Map_Text1,
                                Map_Text2,
                                Map_Text3,
                                Map_Text4,
                                Map_Text5,
                                Map_Text6,
                                Map_Text7,
                                Map_Text8,
                                Create_Date,
                                Create_User,
                                Modify_Date,
                                Modify_User
                            FROM MDM_CDC_Config
                            WHERE DimTypeID = @DimTypeID AND DimID = @DimID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@DimTypeID", SqlDbType.Int) { Value = dimTypeID },
                    new SqlParameter("@DimID", SqlDbType.Int) { Value = dimID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_CDC_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}