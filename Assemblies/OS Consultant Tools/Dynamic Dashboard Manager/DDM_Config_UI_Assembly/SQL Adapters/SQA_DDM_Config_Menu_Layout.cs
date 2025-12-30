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
    public class SQA_DDM_Config_Menu_Layout
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config_Menu_Layout(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_DDM_Config_Menu_Layout_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        public void Update_DDM_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // 1. Define the full Insert Query
                string insertQuery = @"
                INSERT INTO DDM_Config_Menu_Layout
                       (DDM_Config_ID, DDM_Menu_ID, DDM_Type, Scen_Type, Profile_Key, Workspace_ID, MaintUnit_ID, 
                        Sort_Order, Name, Option_Type, Top_Option_Type, TopLeft_Option_Type, TopRight_Option_Type, 
                        Bottom_Option_Type, BottomLeft_Option_Type, BottomRight_Option_Type, Left_Option_Type, 
                        Right_Option_Type, Top_Height, Left_Width, Custom_DB_Header, Custom_DB_Content, 
                        DB_Name, CV_Name, DB_Name_Top, CV_Name_Top, DB_Name_Bottom, CV_Name_Bottom, 
                        DB_Name_TopLeft, CV_Name_TopLeft, DB_Name_TopRight, CV_Name_TopRight, 
                        DB_Name_BottomLeft, CV_Name_BottomLeft, DB_Name_BottomRight, CV_Name_BottomRight, 
                        Status, Create_Date, Create_User, Update_Date, Update_User)
                VALUES
                       (@DDM_Config_ID, @DDM_Menu_ID, @DDM_Type, @Scen_Type, @Profile_Key, @Workspace_ID, @MaintUnit_ID, 
                        @Sort_Order, @Name, @Option_Type, @Top_Option_Type, @TopLeft_Option_Type, @TopRight_Option_Type, 
                        @Bottom_Option_Type, @BottomLeft_Option_Type, @BottomRight_Option_Type, @Left_Option_Type, 
                        @Right_Option_Type, @Top_Height, @Left_Width, @Custom_DB_Header, @Custom_DB_Content, 
                        @DB_Name, @CV_Name, @DB_Name_Top, @CV_Name_Top, @DB_Name_Bottom, @CV_Name_Bottom, 
                        @DB_Name_TopLeft, @CV_Name_TopLeft, @DB_Name_TopRight, @CV_Name_TopRight, 
                        @DB_Name_BottomLeft, @CV_Name_BottomLeft, @DB_Name_BottomRight, @CV_Name_BottomRight, 
                        @Status, @Create_Date, @Create_User, @Update_Date, @Update_User)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                AddParameters(sqa.InsertCommand);

                // 2. Define the full Update Query
                string updateQuery = @"
                UPDATE DDM_Config_Menu_Layout
                SET DDM_Config_ID = @DDM_Config_ID,
                    DDM_Type = @DDM_Type,
                    Scen_Type = @Scen_Type,
                    Profile_Key = @Profile_Key,
                    Workspace_ID = @Workspace_ID,
                    MaintUnit_ID = @MaintUnit_ID,
                    Sort_Order = @Sort_Order,
                    Name = @Name,
                    Option_Type = @Option_Type,
                    Top_Option_Type = @Top_Option_Type,
                    TopLeft_Option_Type = @TopLeft_Option_Type,
                    TopRight_Option_Type = @TopRight_Option_Type,
                    Bottom_Option_Type = @Bottom_Option_Type,
                    BottomLeft_Option_Type = @BottomLeft_Option_Type,
                    BottomRight_Option_Type = @BottomRight_Option_Type,
                    Left_Option_Type = @Left_Option_Type,
                    Right_Option_Type = @Right_Option_Type,
                    Top_Height = @Top_Height,
                    Left_Width = @Left_Width,
                    Custom_DB_Header = @Custom_DB_Header,
                    Custom_DB_Content = @Custom_DB_Content,
                    DB_Name = @DB_Name,
                    CV_Name = @CV_Name,
                    DB_Name_Top = @DB_Name_Top,
                    CV_Name_Top = @CV_Name_Top,
                    DB_Name_Bottom = @DB_Name_Bottom,
                    CV_Name_Bottom = @CV_Name_Bottom,
                    DB_Name_TopLeft = @DB_Name_TopLeft,
                    CV_Name_TopLeft = @CV_Name_TopLeft,
                    DB_Name_TopRight = @DB_Name_TopRight,
                    CV_Name_TopRight = @CV_Name_TopRight,
                    DB_Name_BottomLeft = @DB_Name_BottomLeft,
                    CV_Name_BottomLeft = @CV_Name_BottomLeft,
                    DB_Name_BottomRight = @DB_Name_BottomRight,
                    CV_Name_BottomRight = @CV_Name_BottomRight,
                    Status = @Status,
                    Update_Date = @Update_Date,
                    Update_User = @Update_User
                WHERE DDM_Menu_ID = @DDM_Menu_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                AddParameters(sqa.UpdateCommand);
                sqa.UpdateCommand.Parameters["@DDM_Menu_ID"].SourceVersion = DataRowVersion.Original;

                // 3. Define the Delete Query
                string deleteQuery = "DELETE FROM DDM_Config_Menu_Layout WHERE DDM_Menu_ID = @DDM_Menu_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
                sqa.DeleteCommand.Parameters["@DDM_Menu_ID"].SourceVersion = DataRowVersion.Original;

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
                finally
                {
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
            }
        }

        private void AddParameters(SqlCommand cmd)
        {
            // Identity/Keys
            cmd.Parameters.Add("@DDM_Config_ID", SqlDbType.Int).SourceColumn = "DDM_Config_ID";
            cmd.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
            
            // Core Config
            cmd.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
            cmd.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
            cmd.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
            cmd.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
            cmd.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
            cmd.Parameters.Add("@Sort_Order", SqlDbType.Int).SourceColumn = "Sort_Order";
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).SourceColumn = "Name";
            
            // Option Types (Updated to Int per DDL)
            cmd.Parameters.Add("@Option_Type", SqlDbType.Int).SourceColumn = "Option_Type";
            cmd.Parameters.Add("@Top_Option_Type", SqlDbType.Int).SourceColumn = "Top_Option_Type";
            cmd.Parameters.Add("@TopLeft_Option_Type", SqlDbType.Int).SourceColumn = "TopLeft_Option_Type";
            cmd.Parameters.Add("@TopRight_Option_Type", SqlDbType.Int).SourceColumn = "TopRight_Option_Type";
            cmd.Parameters.Add("@Bottom_Option_Type", SqlDbType.Int).SourceColumn = "Bottom_Option_Type";
            cmd.Parameters.Add("@BottomLeft_Option_Type", SqlDbType.Int).SourceColumn = "BottomLeft_Option_Type";
            cmd.Parameters.Add("@BottomRight_Option_Type", SqlDbType.Int).SourceColumn = "BottomRight_Option_Type";
            cmd.Parameters.Add("@Left_Option_Type", SqlDbType.Int).SourceColumn = "Left_Option_Type";
            cmd.Parameters.Add("@Right_Option_Type", SqlDbType.Int).SourceColumn = "Right_Option_Type";

            // Layout Dimensions
            cmd.Parameters.Add("@Top_Height", SqlDbType.NVarChar, 255).SourceColumn = "Top_Height";
            cmd.Parameters.Add("@Left_Width", SqlDbType.NVarChar, 255).SourceColumn = "Left_Width";

            // Content
            cmd.Parameters.Add("@Custom_DB_Header", SqlDbType.NVarChar, 255).SourceColumn = "Custom_DB_Header";
            cmd.Parameters.Add("@Custom_DB_Content", SqlDbType.NVarChar, -1).SourceColumn = "Custom_DB_Content";
            cmd.Parameters.Add("@DB_Name", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name";
            cmd.Parameters.Add("@CV_Name", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name";
            
            // Quadrant Columns
            cmd.Parameters.Add("@DB_Name_Top", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_Top";
            cmd.Parameters.Add("@CV_Name_Top", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_Top";
            cmd.Parameters.Add("@DB_Name_Bottom", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_Bottom";
            cmd.Parameters.Add("@CV_Name_Bottom", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_Bottom";
            cmd.Parameters.Add("@DB_Name_TopLeft", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_TopLeft";
            cmd.Parameters.Add("@CV_Name_TopLeft", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_TopLeft";
            cmd.Parameters.Add("@DB_Name_TopRight", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_TopRight";
            cmd.Parameters.Add("@CV_Name_TopRight", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_TopRight";
            cmd.Parameters.Add("@DB_Name_BottomLeft", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_BottomLeft";
            cmd.Parameters.Add("@CV_Name_BottomLeft", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_BottomLeft";
            cmd.Parameters.Add("@DB_Name_BottomRight", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_BottomRight";
            cmd.Parameters.Add("@CV_Name_BottomRight", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_BottomRight";

            // Metadata
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
            cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
            cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
        }
    }
}