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

        public void Fill_MDM_CDC_Config_Detail_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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

        public void Update_MDM_CDC_Config_Detail(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "MDM_CDC_Config_Detail", dt);
                    builder.SetPrimaryKey("CDC_Config_ID", "CDC_Config_Detail_ID");
                    builder.ExcludeFromUpdate("CDC_Config_ID", "CDC_Config_Detail_ID", "Create_Date", "Create_User");
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
                                Name,
                                Dim_Type,
                                Dim_ID,
                                Src_Connection,
                                Src_SQL_String,
                                Dim_Mgmt_Process,
                                Trx_Rule,
                                Appr_ID,
                                Mbr_PrefSuff,
                                Mbr_PrefSuff_Txt,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
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
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_CDC_Config_By_Dimension(SessionInfo si, string dimType, int dimID)
        {
            try
            {
                var dt = new DataTable("CDC_Config");
                var sql = @"SELECT 
                                CDC_Config_ID,
                                Name,
                                Dim_Type,
                                Dim_ID,
                                Src_Connection,
                                Src_SQL_String,
                                Dim_Mgmt_Process,
                                Trx_Rule,
                                Appr_ID,
                                Mbr_PrefSuff,
                                Mbr_PrefSuff_Txt,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM MDM_CDC_Config
                            WHERE Dim_Type = @Dim_Type AND Dim_ID = @Dim_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Dim_Type", SqlDbType.NVarChar, 50) { Value = dimType },
                    new SqlParameter("@Dim_ID", SqlDbType.Int) { Value = dimID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_CDC_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_CDC_Config_Detail_By_Config_ID(SessionInfo si, int cdcConfigID)
        {
            try
            {
                var dt = new DataTable("CDC_Config_Detail");
                var sql = @"SELECT 
                                CDC_Config_ID,
                                CDC_Config_Detail_ID,
                                OS_Mbr_Column,
                                OS_Mbr_Vary_Scen_Column,
                                OS_Mbr_Vary_Time_Column,
                                Src_Mbr_Column,
                                Src_Vary_Scen_Column,
                                Src_Vary_Time_Column,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM MDM_CDC_Config_Detail
                            WHERE CDC_Config_ID = @CDC_Config_ID
                            ORDER BY CDC_Config_Detail_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { Value = cdcConfigID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_CDC_Config_Detail_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}