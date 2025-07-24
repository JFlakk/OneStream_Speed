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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Config_Import
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case DashboardExtenderFunctionType.LoadDashboard:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Load Dashboard logic here.
							if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
							{
								var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
								loadDashboardTaskResult.ModifiedCustomSubstVars = null;
								return loadDashboardTaskResult;
							}
						}
						break;
					case DashboardExtenderFunctionType.ComponentSelectionChanged:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Dashboard Component Selection Changed logic here.
							var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
							selectionChangedTaskResult.IsOK = true;
							selectionChangedTaskResult.ShowMessageBox = false;
							selectionChangedTaskResult.Message = "";
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = false;
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = null;
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = false;
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = null;
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = false;
							selectionChangedTaskResult.ModifiedCustomSubstVars = null;
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = false;
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = null;
							return selectionChangedTaskResult;
						}
						break;
					case DashboardExtenderFunctionType.SqlTableEditorSaveData:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement SQL Table Editor Save Data logic here.
							// Save the data rows.
							// XFSqlTableEditorSaveDataTaskInfo saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
							// using (DbConnInfo dbConn = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName))
							// {
								// dbConn.BeginTrans();
								// BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, true, false, false);
								// dbConn.CommitTrans();
							// }

							var saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
							saveDataTaskResult.IsOK = true;
							saveDataTaskResult.ShowMessageBox = false;
							saveDataTaskResult.Message = "";
							saveDataTaskResult.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
							return saveDataTaskResult;
						}
						break;
				}
				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}
	}
}


// To run this code, you would need to have a C# project (.NET 6 or later recommended)
// and add the System.Data.SqlClient package.
// You can add it via NuGet: dotnet add package System.Data.SqlClient

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Xml;

namespace DDM_DataMigration
{
    // ==========================================================================================
    // DATA MODELS (V2 - Keyless)
    // Description: These classes represent your database tables for serialization.
    //              Primary key fields (e.g., DDM_Profile_ID) are omitted for export.
    // ==========================================================================================

    [XmlRoot("DDM_Profiles")]
    public class DDM_Profiles
    {
        [XmlElement("DDM_Profile")]
        public List<DDM_Profile> Profiles { get; set; } = new List<DDM_Profile>();
    }

    public class DDM_Profile
    {
        // Business Key: ProfileKey
        [XmlAttribute]
        public Guid ProfileKey { get; set; }

        [XmlAttribute]
        public string Profile_Step_Type { get; set; }
        [XmlAttribute]
        public string Status { get; set; }
        [XmlAttribute]
        public DateTime Create_Date { get; set; }
        [XmlAttribute]
        public string Create_User { get; set; }
        [XmlAttribute]
        public DateTime? Update_Date { get; set; }
        [XmlAttribute]
        public string Update_User { get; set; }

        [XmlArray("Menus")]
        [XmlArrayItem("Menu")]
        public List<DDM_Menu> Menus { get; set; } = new List<DDM_Menu>();
    }

    public class DDM_Menu
    {
        // Business Key: Name (within a Profile)
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Sort_Order { get; set; }
        [XmlAttribute]
        public string Option_Type { get; set; }
        [XmlAttribute]
        public string Custom_DB_Header { get; set; }
        [XmlAttribute]
        public string Custom_DB_Content { get; set; }
        [XmlAttribute]
        public string DB_Name { get; set; }
        [XmlAttribute]
        public string CV_Name { get; set; }
        [XmlAttribute]
        public string Status { get; set; }
        [XmlAttribute]
        public DateTime Create_Date { get; set; }
        [XmlAttribute]
        public string Create_User { get; set; }
        [XmlAttribute]
        public DateTime? Update_Date { get; set; }
        [XmlAttribute]
        public string Update_User { get; set; }

        [XmlArray("Headers")]
        [XmlArrayItem("Header")]
        public List<DDM_Header> Headers { get; set; } = new List<DDM_Header>();
    }

    public class DDM_Header
    {
        // Business Key: A combination of Option_Type and a relevant label/parameter.
        [XmlAttribute]
        public int? Sort_Order { get; set; }
        [XmlAttribute]
        public string Option_Type { get; set; }
        [XmlAttribute]
        public string Fltr_Dim_Type { get; set; }
        [XmlAttribute]
        public string Fltr_Dim_Name { get; set; }
        [XmlAttribute]
        public string Fltr_MFB { get; set; }
        [XmlAttribute]
        public string Fltr_Default { get; set; }
        [XmlAttribute]
        public bool Fltr_Btn { get; set; }
        [XmlAttribute]
        public string Fltr_Btn_Lbl { get; set; }
        [XmlAttribute]
        public string Fltr_Btn_ToolTip { get; set; }
        [XmlAttribute]
        public bool Fltr_Cbx { get; set; }
        [XmlAttribute]
        public string Fltr_Cbx_Lbl { get; set; }
        [XmlAttribute]
        public string Fltr_Cbx_ToolTip { get; set; }
        [XmlAttribute]
        public string Fltr_BtnCbx_BoundParam { get; set; }
        [XmlAttribute]
        public bool Fltr_Txt { get; set; }
        [XmlAttribute]
        public string Fltr_Txt_Lbl { get; set; }
        [XmlAttribute]
        public string Fltr_Txt_ToolTip { get; set; }
        [XmlAttribute]
        public string Fltr_Txt_BoundParam { get; set; }
        [XmlAttribute]
        public string Btn_Type { get; set; }
        [XmlAttribute]
        public string Btn_Lbl { get; set; }
        [XmlAttribute]
        public string Btn_ToolTip { get; set; }
        [XmlAttribute]
        public string Btn_Image_URL { get; set; }
        [XmlAttribute]
        public string Btn_ServerTask { get; set; }
        [XmlAttribute]
        public string Btn_DBOpen { get; set; }
        [XmlAttribute]
        public string Btn_DBRefresh { get; set; }
        [XmlAttribute]
        public DateTime Create_Date { get; set; }
        [XmlAttribute]
        public string Create_User { get; set; }
        [XmlAttribute]
        public DateTime? Update_Date { get; set; }
        [XmlAttribute]
        public string Update_User { get; set; }
    }


    // ==========================================================================================
    // MIGRATION SERVICE (V2)
    // Description: This class contains the logic for exporting and importing the data,
    //              now with keyless export and upsert (Update/Insert) import logic.
    // ==========================================================================================
    public class DdmMigrationService
    {
        /// <summary>
        /// Exports DDM data from the database to an XML file, omitting primary keys.
        /// </summary>
        public void ExportToXml(string connectionString, string filePath)
        {
            Console.WriteLine("Starting keyless data export...");

            var profiles = new List<DDM_Profile>();
            
            // Temporary storage for full data including IDs to build the hierarchy
            var profilesWithIds = new List<(int id, DDM_Profile profile)>();
            var menusWithIds = new List<(int id, int parentId, DDM_Menu menu)>();
            var headersWithIds = new List<(int id, int parentId, DDM_Header header)>();

            using (var connection = new SqlConnection(connectionString))
            {
                var commandText = @"
                    SELECT * FROM [dbo].[DDM_Config];
                    SELECT * FROM [dbo].[DDM_Config_Menu];
                    SELECT * FROM [dbo].[DDM_Config_Menu_Hdr];
                ";
                var command = new SqlCommand(commandText, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) profilesWithIds.Add(MapReaderToProfileWithId(reader));
                    reader.NextResult();
                    while (reader.Read()) menusWithIds.Add(MapReaderToMenuWithId(reader));
                    reader.NextResult();
                    while (reader.Read()) headersWithIds.Add(MapReaderToHeaderWithId(reader));
                }
            }

            // Assemble the hierarchy in memory using the IDs
            foreach (var p in profilesWithIds)
            {
                var profileMenus = menusWithIds.Where(m => m.parentId == p.id).ToList();
                foreach (var m in profileMenus)
                {
                    m.menu.Headers = headersWithIds.Where(h => h.parentId == m.id).Select(h => h.header).ToList();
                }
                p.profile.Menus = profileMenus.Select(m => m.menu).ToList();
                profiles.Add(p.profile);
            }

            // Serialize to XML
            var dataToSerialize = new DDM_Profiles { Profiles = profiles };
            var serializer = new XmlSerializer(typeof(DDM_Profiles));
            using (var writer = new StreamWriter(filePath))
            {
                var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true });
                serializer.Serialize(xmlWriter, dataToSerialize);
            }

            Console.WriteLine($"Export complete. Data saved to {filePath}");
        }

        /// <summary>
        /// Imports DDM data from an XML file into the database using upsert logic.
        /// </summary>
        public void ImportFromXml(string connectionString, string filePath)
        {
            Console.WriteLine("Starting data import with upsert logic...");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found at {filePath}");
                return;
            }

            // Deserialize from XML
            DDM_Profiles dataToImport;
            var serializer = new XmlSerializer(typeof(DDM_Profiles));
            using (var reader = new StreamReader(filePath))
            {
                dataToImport = (DDM_Profiles)serializer.Deserialize(reader);
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var profile in dataToImport.Profiles)
                        {
                            // 1. Upsert Profile
                            var profileId = UpsertProfile(profile, connection, transaction);

                            foreach (var menu in profile.Menus)
                            {
                                // 2. Upsert Menu
                                var menuId = UpsertMenu(menu, profileId, connection, transaction);

                                foreach (var header in menu.Headers)
                                {
                                    // 3. Upsert Header
                                    UpsertHeader(header, profileId, menuId, connection, transaction);
                                }
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine("Import complete. Transaction committed.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred during import. Rolling back transaction.");
                        Console.WriteLine(ex.ToString());
                        transaction.Rollback();
                    }
                }
            }
        }

        #region Upsert Logic
        private int UpsertProfile(DDM_Profile profile, SqlConnection conn, SqlTransaction tran)
        {
            // Check for existence using the business key (ProfileKey)
            var checkCmd = new SqlCommand("SELECT DDM_Profile_ID FROM [dbo].[DDM_Config] WHERE ProfileKey = @ProfileKey", conn, tran);
            checkCmd.Parameters.AddWithValue("@ProfileKey", profile.ProfileKey);
            var existingId = checkCmd.ExecuteScalar();

            if (existingId != null)
            {
                // UPDATE
                Console.WriteLine($"Updating Profile with Key: {profile.ProfileKey}");
                var updateCmd = new SqlCommand(@"
                    UPDATE [dbo].[DDM_Config] SET 
                        Profile_Step_Type = @Profile_Step_Type, Status = @Status, Create_Date = @Create_Date, 
                        Create_User = @Create_User, Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE DDM_Profile_ID = @ID;", conn, tran);
                
                updateCmd.Parameters.AddWithValue("@ID", (int)existingId);
                // Add other parameters
                updateCmd.Parameters.AddWithValue("@Profile_Step_Type", profile.Profile_Step_Type);
                updateCmd.Parameters.AddWithValue("@Status", profile.Status);
                updateCmd.Parameters.AddWithValue("@Create_Date", profile.Create_Date);
                updateCmd.Parameters.AddWithValue("@Create_User", profile.Create_User);
                updateCmd.Parameters.AddWithValue("@Update_Date", (object)profile.Update_Date ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Update_User", (object)profile.Update_User ?? DBNull.Value);
                updateCmd.ExecuteNonQuery();
                return (int)existingId;
            }
            else
            {
                // INSERT
                Console.WriteLine($"Inserting Profile with Key: {profile.ProfileKey}");
                var insertCmd = new SqlCommand(@"
                    INSERT INTO [dbo].[DDM_Config] (ProfileKey, Profile_Step_Type, Status, Create_Date, Create_User, Update_Date, Update_User)
                    OUTPUT INSERTED.DDM_Profile_ID
                    VALUES (@ProfileKey, @Profile_Step_Type, @Status, @Create_Date, @Create_User, @Update_Date, @Update_User);", conn, tran);

                insertCmd.Parameters.AddWithValue("@ProfileKey", profile.ProfileKey);
                // Add other parameters
                insertCmd.Parameters.AddWithValue("@Profile_Step_Type", profile.Profile_Step_Type);
                insertCmd.Parameters.AddWithValue("@Status", profile.Status);
                insertCmd.Parameters.AddWithValue("@Create_Date", profile.Create_Date);
                insertCmd.Parameters.AddWithValue("@Create_User", profile.Create_User);
                insertCmd.Parameters.AddWithValue("@Update_Date", (object)profile.Update_Date ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Update_User", (object)profile.Update_User ?? DBNull.Value);
                return (int)insertCmd.ExecuteScalar();
            }
        }

        private int UpsertMenu(DDM_Menu menu, int profileId, SqlConnection conn, SqlTransaction tran)
        {
            // Check for existence using business key (Name within a Profile)
            var checkCmd = new SqlCommand("SELECT DDM_Menu_ID FROM [dbo].[DDM_Config_Menu] WHERE DDM_Profile_ID = @ProfileID AND Name = @Name", conn, tran);
            checkCmd.Parameters.AddWithValue("@ProfileID", profileId);
            checkCmd.Parameters.AddWithValue("@Name", menu.Name);
            var existingId = checkCmd.ExecuteScalar();

            if (existingId != null)
            {
                // UPDATE
                Console.WriteLine($"  -> Updating Menu: {menu.Name}");
                var updateCmd = new SqlCommand(@"
                    UPDATE [dbo].[DDM_Config_Menu] SET 
                        Sort_Order = @Sort_Order, Option_Type = @Option_Type, Custom_DB_Header = @Custom_DB_Header, 
                        Custom_DB_Content = @Custom_DB_Content, DB_Name = @DB_Name, CV_Name = @CV_Name, 
                        Status = @Status, Create_Date = @Create_Date, Create_User = @Create_User, 
                        Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE DDM_Menu_ID = @ID;", conn, tran);
                
                updateCmd.Parameters.AddWithValue("@ID", (int)existingId);
                // Add other parameters
                updateCmd.Parameters.AddWithValue("@Sort_Order", menu.Sort_Order);
                updateCmd.Parameters.AddWithValue("@Option_Type", (object)menu.Option_Type ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Custom_DB_Header", (object)menu.Custom_DB_Header ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Custom_DB_Content", (object)menu.Custom_DB_Content ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@DB_Name", (object)menu.DB_Name ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@CV_Name", (object)menu.CV_Name ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Status", menu.Status);
                updateCmd.Parameters.AddWithValue("@Create_Date", menu.Create_Date);
                updateCmd.Parameters.AddWithValue("@Create_User", menu.Create_User);
                updateCmd.Parameters.AddWithValue("@Update_Date", (object)menu.Update_Date ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Update_User", (object)menu.Update_User ?? DBNull.Value);
                updateCmd.ExecuteNonQuery();
                return (int)existingId;
            }
            else
            {
                // INSERT
                Console.WriteLine($"  -> Inserting Menu: {menu.Name}");
                var insertCmd = new SqlCommand(@"
                    INSERT INTO [dbo].[DDM_Config_Menu] (DDM_Profile_ID, Sort_Order, Name, Option_Type, Custom_DB_Header, Custom_DB_Content, DB_Name, CV_Name, Status, Create_Date, Create_User, Update_Date, Update_User)
                    OUTPUT INSERTED.DDM_Menu_ID
                    VALUES (@DDM_Profile_ID, @Sort_Order, @Name, @Option_Type, @Custom_DB_Header, @Custom_DB_Content, @DB_Name, @CV_Name, @Status, @Create_Date, @Create_User, @Update_Date, @Update_User);", conn, tran);
                
                insertCmd.Parameters.AddWithValue("@DDM_Profile_ID", profileId);
                // Add other parameters
                insertCmd.Parameters.AddWithValue("@Sort_Order", menu.Sort_Order);
                insertCmd.Parameters.AddWithValue("@Name", menu.Name);
                insertCmd.Parameters.AddWithValue("@Option_Type", (object)menu.Option_Type ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Custom_DB_Header", (object)menu.Custom_DB_Header ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Custom_DB_Content", (object)menu.Custom_DB_Content ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@DB_Name", (object)menu.DB_Name ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@CV_Name", (object)menu.CV_Name ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Status", menu.Status);
                insertCmd.Parameters.AddWithValue("@Create_Date", menu.Create_Date);
                insertCmd.Parameters.AddWithValue("@Create_User", menu.Create_User);
                insertCmd.Parameters.AddWithValue("@Update_Date", (object)menu.Update_Date ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Update_User", (object)menu.Update_User ?? DBNull.Value);
                return (int)insertCmd.ExecuteScalar();
            }
        }

        private void UpsertHeader(DDM_Header header, int profileId, int menuId, SqlConnection conn, SqlTransaction tran)
        {
            // Business key for a header is more complex. We'll use the BoundParam as a unique identifier within a menu.
            // A more robust solution might require a dedicated business key column.
            string uniqueIdentifier = header.Fltr_BtnCbx_BoundParam ?? header.Fltr_Txt_BoundParam ?? header.Btn_Lbl ?? Guid.NewGuid().ToString();
            string checkSql = "SELECT DDM_Menu_Hdr_ID FROM [dbo].[DDM_Config_Menu_Hdr] WHERE DDM_Menu_ID = @MenuID AND (Fltr_BtnCbx_BoundParam = @Key OR Fltr_Txt_BoundParam = @Key OR Btn_Lbl = @Key)";
            
            var checkCmd = new SqlCommand(checkSql, conn, tran);
            checkCmd.Parameters.AddWithValue("@MenuID", menuId);
            checkCmd.Parameters.AddWithValue("@Key", uniqueIdentifier);
            var existingId = checkCmd.ExecuteScalar();
            
            string logIdentifier = $"Header '{uniqueIdentifier}' in Menu ID {menuId}";

            if (existingId != null)
            {
                // UPDATE
                Console.WriteLine($"    -> Updating {logIdentifier}");
                var updateCmd = new SqlCommand(@"
                    UPDATE [dbo].[DDM_Config_Menu_Hdr] SET /* All fields here */ 
                        Sort_Order = @Sort_Order, Option_Type = @Option_Type, Fltr_Dim_Type = @Fltr_Dim_Type, Fltr_Dim_Name = @Fltr_Dim_Name, Fltr_MFB = @Fltr_MFB, Fltr_Default = @Fltr_Default, Fltr_Btn = @Fltr_Btn, Fltr_Btn_Lbl = @Fltr_Btn_Lbl, Fltr_Btn_ToolTip = @Fltr_Btn_ToolTip, Fltr_Cbx = @Fltr_Cbx, Fltr_Cbx_Lbl = @Fltr_Cbx_Lbl, Fltr_Cbx_ToolTip = @Fltr_Cbx_ToolTip, Fltr_BtnCbx_BoundParam = @Fltr_BtnCbx_BoundParam, Fltr_Txt = @Fltr_Txt, Fltr_Txt_Lbl = @Fltr_Txt_Lbl, Fltr_Txt_ToolTip = @Fltr_Txt_ToolTip, Fltr_Txt_BoundParam = @Fltr_Txt_BoundParam, Btn_Type = @Btn_Type, Btn_Lbl = @Btn_Lbl, Btn_ToolTip = @Btn_ToolTip, Btn_Image_URL = @Btn_Image_URL, Btn_ServerTask = @Btn_ServerTask, Btn_DBOpen = @Btn_DBOpen, Btn_DBRefresh = @Btn_DBRefresh, Create_Date = @Create_Date, Create_User = @Create_User, Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE DDM_Menu_Hdr_ID = @ID;", conn, tran);
                AddHeaderParameters(updateCmd, header, profileId, menuId);
                updateCmd.Parameters.AddWithValue("@ID", (int)existingId);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                // INSERT
                Console.WriteLine($"    -> Inserting {logIdentifier}");
                var insertCmd = new SqlCommand(@"
                    INSERT INTO [dbo].[DDM_Config_Menu_Hdr] (DDM_Profile_ID, DDM_Menu_ID, Sort_Order, Option_Type, Fltr_Dim_Type, Fltr_Dim_Name, Fltr_MFB, Fltr_Default, Fltr_Btn, Fltr_Btn_Lbl, Fltr_Btn_ToolTip, Fltr_Cbx, Fltr_Cbx_Lbl, Fltr_Cbx_ToolTip, Fltr_BtnCbx_BoundParam, Fltr_Txt, Fltr_Txt_Lbl, Fltr_Txt_ToolTip, Fltr_Txt_BoundParam, Btn_Type, Btn_Lbl, Btn_ToolTip, Btn_Image_URL, Btn_ServerTask, Btn_DBOpen, Btn_DBRefresh, Create_Date, Create_User, Update_Date, Update_User)
                    VALUES (@DDM_Profile_ID, @DDM_Menu_ID, @Sort_Order, @Option_Type, @Fltr_Dim_Type, @Fltr_Dim_Name, @Fltr_MFB, @Fltr_Default, @Fltr_Btn, @Fltr_Btn_Lbl, @Fltr_Btn_ToolTip, @Fltr_Cbx, @Fltr_Cbx_Lbl, @Fltr_Cbx_ToolTip, @Fltr_BtnCbx_BoundParam, @Fltr_Txt, @Fltr_Txt_Lbl, @Fltr_Txt_ToolTip, @Fltr_Txt_BoundParam, @Btn_Type, @Btn_Lbl, @Btn_ToolTip, @Btn_Image_URL, @Btn_ServerTask, @Btn_DBOpen, @Btn_DBRefresh, @Create_Date, @Create_User, @Update_Date, @Update_User);", conn, tran);
                AddHeaderParameters(insertCmd, header, profileId, menuId);
                insertCmd.ExecuteNonQuery();
            }
        }

        private void AddHeaderParameters(SqlCommand cmd, DDM_Header header, int profileId, int menuId)
        {
            cmd.Parameters.AddWithValue("@DDM_Profile_ID", profileId);
            cmd.Parameters.AddWithValue("@DDM_Menu_ID", menuId);
            cmd.Parameters.AddWithValue("@Sort_Order", (object)header.Sort_Order ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Option_Type", (object)header.Option_Type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Dim_Type", (object)header.Fltr_Dim_Type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Dim_Name", (object)header.Fltr_Dim_Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_MFB", (object)header.Fltr_MFB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Default", (object)header.Fltr_Default ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Btn", header.Fltr_Btn);
            cmd.Parameters.AddWithValue("@Fltr_Btn_Lbl", (object)header.Fltr_Btn_Lbl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Btn_ToolTip", (object)header.Fltr_Btn_ToolTip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Cbx", header.Fltr_Cbx);
            cmd.Parameters.AddWithValue("@Fltr_Cbx_Lbl", (object)header.Fltr_Cbx_Lbl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Cbx_ToolTip", (object)header.Fltr_Cbx_ToolTip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_BtnCbx_BoundParam", (object)header.Fltr_BtnCbx_BoundParam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Txt", header.Fltr_Txt);
            cmd.Parameters.AddWithValue("@Fltr_Txt_Lbl", (object)header.Fltr_Txt_Lbl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Txt_ToolTip", (object)header.Fltr_Txt_ToolTip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fltr_Txt_BoundParam", (object)header.Fltr_Txt_BoundParam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_Type", (object)header.Btn_Type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_Lbl", (object)header.Btn_Lbl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_ToolTip", (object)header.Btn_ToolTip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_Image_URL", (object)header.Btn_Image_URL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_ServerTask", (object)header.Btn_ServerTask ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_DBOpen", (object)header.Btn_DBOpen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Btn_DBRefresh", (object)header.Btn_DBRefresh ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Create_Date", header.Create_Date);
            cmd.Parameters.AddWithValue("@Create_User", header.Create_User);
            cmd.Parameters.AddWithValue("@Update_Date", (object)header.Update_Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Update_User", (object)header.Update_User ?? DBNull.Value);
        }
        #endregion

        #region Data Reader Mappers (for Export)
        private (int, DDM_Profile) MapReaderToProfileWithId(SqlDataReader reader) => ((int)reader["DDM_Profile_ID"], new DDM_Profile
        {
            ProfileKey = (Guid)reader["ProfileKey"],
            Profile_Step_Type = (string)reader["Profile_Step_Type"],
            Status = (string)reader["Status"],
            Create_Date = (DateTime)reader["Create_Date"],
            Create_User = (string)reader["Create_User"],
            Update_Date = reader["Update_Date"] as DateTime?,
            Update_User = reader["Update_User"] as string
        });

        private (int, int, DDM_Menu) MapReaderToMenuWithId(SqlDataReader reader) => ((int)reader["DDM_Menu_ID"], (int)reader["DDM_Profile_ID"], new DDM_Menu
        {
            Sort_Order = (int)reader["Sort_Order"],
            Name = (string)reader["Name"],
            Option_Type = reader["Option_Type"] as string,
            Custom_DB_Header = reader["Custom_DB_Header"] as string,
            Custom_DB_Content = reader["Custom_DB_Content"] as string,
            DB_Name = reader["DB_Name"] as string,
            CV_Name = reader["CV_Name"] as string,
            Status = (string)reader["Status"],
            Create_Date = (DateTime)reader["Create_Date"],
            Create_User = (string)reader["Create_User"],
            Update_Date = reader["Update_Date"] as DateTime?,
            Update_User = reader["Update_User"] as string
        });

        private (int, int, DDM_Header) MapReaderToHeaderWithId(SqlDataReader reader) => ((int)reader["DDM_Menu_Hdr_ID"], (int)reader["DDM_Menu_ID"], new DDM_Header
        {
            Sort_Order = reader["Sort_Order"] as int?,
            Option_Type = reader["Option_Type"] as string,
            Fltr_Dim_Type = reader["Fltr_Dim_Type"] as string,
            Fltr_Dim_Name = reader["Fltr_Dim_Name"] as string,
            Fltr_MFB = reader["Fltr_MFB"] as string,
            Fltr_Default = reader["Fltr_Default"] as string,
            Fltr_Btn = (bool)reader["Fltr_Btn"],
            Fltr_Btn_Lbl = reader["Fltr_Btn_Lbl"] as string,
            Fltr_Btn_ToolTip = reader["Fltr_Btn_ToolTip"] as string,
            Fltr_Cbx = (bool)reader["Fltr_Cbx"],
            Fltr_Cbx_Lbl = reader["Fltr_Cbx_Lbl"] as string,
            Fltr_Cbx_ToolTip = reader["Fltr_Cbx_ToolTip"] as string,
            Fltr_BtnCbx_BoundParam = reader["Fltr_BtnCbx_BoundParam"] as string,
            Fltr_Txt = (bool)reader["Fltr_Txt"],
            Fltr_Txt_Lbl = reader["Fltr_Txt_Lbl"] as string,
            Fltr_Txt_ToolTip = reader["Fltr_Txt_ToolTip"] as string,
            Fltr_Txt_BoundParam = reader["Fltr_Txt_BoundParam"] as string,
            Btn_Type = reader["Btn_Type"] as string,
            Btn_Lbl = reader["Btn_Lbl"] as string,
            Btn_ToolTip = reader["Btn_ToolTip"] as string,
            Btn_Image_URL = reader["Btn_Image_URL"] as string,
            Btn_ServerTask = reader["Btn_ServerTask"] as string,
            Btn_DBOpen = reader["Btn_DBOpen"] as string,
            Btn_DBRefresh = reader["Btn_DBRefresh"] as string,
            Create_Date = (DateTime)reader["Create_Date"],
            Create_User = (string)reader["Create_User"],
            Update_Date = reader["Update_Date"] as DateTime?,
            Update_User = reader["Update_User"] as string
        });
        #endregion
    }

    // ==========================================================================================
    // EXAMPLE USAGE
    // ==========================================================================================
    class Program
    {
        static void Main(string[] args)
        {
            // !!! IMPORTANT: Replace with your actual connection strings !!!
            var sourceConnectionString = "Server=SOURCE_SERVER;Database=OneStream_PPBE_DOD;Trusted_Connection=True;";
            var destinationConnectionString = "Server=DESTINATION_SERVER;Database=OneStream_PPBE_DOD;Trusted_Connection=True;";
            var filePath = "C:\\Temp\\DDM_Export_V2.xml";

            var migrationService = new DdmMigrationService();

            // --- To run an export ---
            // migrationService.ExportToXml(sourceConnectionString, filePath);

            // --- To run an import ---
            // migrationService.ImportFromXml(destinationConnectionString, filePath);

            Console.WriteLine("Process finished. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
