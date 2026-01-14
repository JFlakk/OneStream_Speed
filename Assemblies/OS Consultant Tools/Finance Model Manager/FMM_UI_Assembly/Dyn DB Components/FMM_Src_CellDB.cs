using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
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
    public class FMM_Src_CellDB
    {
        // Database table that will contain our objects
        public string TableName { get; } = "FMM_Src_Cell";

        // SessionInfo instance
        public SessionInfo SI { get; }

        // constructor
        public FMM_Src_CellDB(SessionInfo si)
        {
            this.si = si;
        }



        // Update-or-insert function to create records
        public ConfigModel Upsert(Guid configId, Guid wfProfileId, ScenarioTypeId aScenarioTypeID, string aFrequency, Guid defaultDashboard, Guid matchDashboard)
        {
            try
            {
                string sql = $@"
IF EXISTS (SELECT conf.confID FROM {this.TableName} AS conf WITH (NOLOCK) WHERE conf.confID = @confID)
BEGIN
    UPDATE
         conf
    SET
         conf.wfProfileKey = @wfID
        ,conf.ScenarioTypeID = @scenarioTypeID
        ,conf.Frequency = @frequency
        ,conf.DefaultDashboardID = @defDbID
        ,conf.MatchDashboardID = @matchDbID
    FROM
         {this.TableName} As Conf
    WHERE
         Conf.ConfID = @ConfID
END
ELSE
BEGIN
    INSERT Into {this.TableName}
    (
        confID
        ,wfProfileKey
        ,ScenarioTypeID 
        ,Frequency 
        ,DefaultDashboardID 
        ,MatchDashboardID 
    )
    OUTPUT Inserted.ConfID
    VALUES
    (
        NEWID()
        ,@wfID
        ,@scenarioTypeID
        ,@frequency
        ,@defDbID
        ,@matchDbID
    );
END";
                List<DbParamInfo> paramList = new List<DbParamInfo>
            {
                new DbParamInfo("@confID", configId),
                new DbParamInfo("@wfID", wfProfileId),
                new DbParamInfo("@scenarioTypeID", aScenarioTypeID),
                new DbParamInfo("@frequency", aFrequency),
                new DbParamInfo("@defDbID", defaultDashboard),
                new DbParamInfo("@matchDbId", matchDashboard)
            };

                DataTable output = null;
                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
                {
                    output = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    if (output.Rows.Count > 0)
                    {
                        return this.GetConfig((Guid)output.Rows[0][0]);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogError(this.SI, ex);
            }
            return null;
        }

        // Update-or-insert function to persist objects
        public ConfigModel Upsert(ConfigModel config)
        {
            return this.Upsert(config.ID, config.WfID, config.ScenarioTypeID, config.Frequency, config.DefaultDashboardID, config.MatchDashboardID);
        }

        // Create new record with default values
        public void CreateDefaultConfig(Guid workspaceID)
        {
            this.Upsert(Guid.Empty, Guid.Empty, ScenarioTypeId.All, "Monthly", Guid.Empty, Guid.Empty);
        }

        // Retrieve a single object
        public ConfigModel GetSRCCells(Guid configID)
        {
            string sql = $"SELECT * FROM {this.TableName} WHERE confID = @confID";
            List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@confID", configID) };
            using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
            {
                DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                if (dt.Rows.Count < 1) return null;
                DataRow dr = dt.Rows[0];
                return new ConfigModel
                {
                    ID = (Guid)dr["ConfID"],
                    WfID = (Guid)dr["wfProfileKey"],
                    ScenarioTypeID = ScenarioTypeId.Parse<ScenarioTypeId>(dr["ScenarioTypeID"].ToString()),
                    Frequency = dr["Frequency"].ToString(),
                    DefaultDashboardID = (Guid)dr["DefaultDashboardID"],
                    MatchDashboardID = (Guid)dr["MatchDashboardID"]
                };
            }
        }

        // Retrieve all ConfigModel objects
        public List<ConfigModel> GetSRCCells()
        {
            var sql = $"SELECT * FROM {this.TableName} ORDER BY wfProfileKey ASC, ScenarioTypeID ASC";
            using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
            {
                DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, false);
                List<ConfigModel> result = new List<ConfigModel>();
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new ConfigModel
                    {
                        ID = (Guid)dr["ConfID"],
                        WfID = (Guid)dr["wfProfileKey"],
                        ScenarioTypeID = ScenarioTypeId.Parse<ScenarioTypeId>(dr["ScenarioTypeID"].ToString()),
                        Frequency = dr["Frequency"].ToString(),
                        DefaultDashboardID = (Guid)dr["DefaultDashboardID"],
                        MatchDashboardID = (Guid)dr["MatchDashboardID"]
                    });
                }
                return result;
            }
        }

        // Retrieve ConfigModel objects by Workflow Profile and Scenario Type
        public List<ConfigModel> GetConfigsByWfAndScenarioType(Guid wkProfileKey, int scenTypeId)
        {
            string sql = $@"
SELECT * FROM {this.TableName} 
WHERE wfProfileKey = @wfID AND (ScenarioTypeID = @sTypeID 
OR ScenarioTypeID = {ScenarioType.All.Id})
ORDER BY wfProfileKey ASC, ScenarioTypeID ASC";

            List<DbParamInfo> dbParams = new List<DbParamInfo>
        {
            new DbParamInfo("@wfID", wkProfileKey),
            new DbParamInfo("@sTypeID", scenTypeId)
        };
            using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
            {
                DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, dbParams, false);
                List<ConfigModel> result = new List<ConfigModel>();
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new ConfigModel
                    {
                        ID = (Guid)dr["ConfID"],
                        WfID = (Guid)dr["wfProfileKey"],
                        ScenarioTypeID = ScenarioTypeId.Parse<ScenarioTypeId>(dr["ScenarioTypeID"].ToString()),
                        Frequency = dr["Frequency"].ToString(),
                        DefaultDashboardID = (Guid)dr["DefaultDashboardID"],
                        MatchDashboardID = (Guid)dr["MatchDashboardID"]
                    });
                }
                return result;
            }
        }

        // Return one profile for the current wfclusterpk
        public ConfigModel GetConfigForWorkflowUnitClusterPk(WorkflowUnitClusterPk wfUnitClusterPK)
        {
            int scenTypeId = BRApi.Finance.Scenario.GetScenarioType(SI, wfUnitClusterPK.ScenarioKey).Id;
            WorkflowTrackingFrequency freq = BRApi.Workflow.General.GetWorkflowUnitTrackingFrequency(SI, wfUnitClusterPK);
            // todo: filter by wf frequency, to privilege matching ones
            List<ConfigModel> availableConfigs = this.GetConfigsByWfAndScenarioType(wfUnitClusterPK.ProfileKey, BRApi.Finance.Scenario.GetScenarioType(SI, wfUnitClusterPK.ScenarioKey).Id);
            if (availableConfigs.Count > 0)
            {
                return availableConfigs[0];
            }
            return null;
        }

        // Delete an object by id
        public void Delete(Guid confId)
        {
            string sql = $"DELETE FROM {this.TableName} WHERE confID = @confID";
            List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@confID", confId) };
            using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
            {
                BRApi.Database.ExecuteActionQuery(dbConn, sql, paramList, false, true);
            }
        }

        // Delete an object by instance
        public void Delete(ConfigModel config)
        {
            this.Delete(config.ID);
        }
    }
}
