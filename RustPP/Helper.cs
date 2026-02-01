using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Fougerite;
using RustPP.Commands;
using RustPP.Permissions;

namespace RustPP
{
    public static class Helper
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        public static void Log(string logName, string msg)
        {
            File.AppendAllText(RustPPModule.GetAbsoluteFilePath(logName),
                string.Format("[{0} {1}] {2}\r\n", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(),
                    msg));
        }

        public static void CreateSaves()
        {
            ShareCommand shareCmd = (ShareCommand)ChatCommand.GetCommand("share");
            FriendsCommand friendsCmd = (FriendsCommand)ChatCommand.GetCommand("friends");

            // Save Doors
            if (shareCmd.GetSharedDoors().Count != 0)
            {
                var doorsData = new Dictionary<ulong, List<ulong>>();
                foreach (DictionaryEntry entry in shareCmd.GetSharedDoors())
                {
                    doorsData[(ulong)entry.Key] = new List<ulong>(((ArrayList)entry.Value).OfType<ulong>());
                }

                SaveJson("doorsSave.json", doorsData);
            }

            // Save Friends
            if (friendsCmd.GetFriendsLists().Count != 0)
                SaveJson("friendsSave.json", friendsCmd.GetFriendsLists());

            // Save Admins
            if (Administrator.AdminList.Count != 0)
                SaveJson("admins.json", Administrator.AdminList);

            // Save Cache
            if (Core.userCache.Count != 0)
                SaveJson("userCache.json", Core.userCache);

            // Save PLists
            if (Core.whiteList.Count != 0) 
                SaveJson("whitelist.json", Core.whiteList.PlayerList);
            if (Core.muteList.Count != 0) 
                SaveJson("mutelist.json", Core.muteList.PlayerList);
            if (Core.blackList.Count != 0) 
                SaveJson("bans.json", Core.blackList.PlayerList);
        }

        public static void SaveJson<T>(string fileName, T obj)
        {
            try
            {
                string path = RustPPModule.GetAbsoluteFilePath(fileName);
                string json = JsonConvert.SerializeObject(obj, JsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Rust++] JSON Save Error ({fileName}): {ex.Message}");
            }
        }

        public static T LoadWithMigration<T, TLegacyXML>(string jsonFile, string xmlFile, string rppFile = null)
        {
            string jsonPath = RustPPModule.GetAbsoluteFilePath(jsonFile);
            string xmlPath = RustPPModule.GetAbsoluteFilePath(xmlFile);
            string rppPath = rppFile != null ? RustPPModule.GetAbsoluteFilePath(rppFile) : null;

            T data = default(T);
            bool isLoaded = false;

            // Load JSON if exists
            if (File.Exists(jsonPath))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<T>(File.ReadAllText(jsonPath), JsonSettings);
                    isLoaded = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[Rust++] JSON load failed: {jsonPath} {ex}");
                }
            }

            // Migration Fallback XML
            if (!isLoaded && File.Exists(xmlPath))
            {
                try
                {
                    var xmlObj = ObjectFromXML<TLegacyXML>(xmlPath);
                    if (xmlObj is SerializableDictionary<ulong, List<ulong>> sDict)
                        data = (T)(object)new Dictionary<ulong, List<ulong>>(sDict);
                    else if (xmlObj is SerializableDictionary<ulong, string> cDict)
                        data = (T)(object)new Dictionary<ulong, string>(cDict);
                    else
                        data = (T)(object)xmlObj;
                    isLoaded = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[Rust++] XML Migration failed: {xmlFile} {ex}");
                }
            }

            // Migration Fallback RPP (Binary)
            if (!isLoaded && rppPath != null && File.Exists(rppPath))
            {
                try
                {
                    data = ObjectFromFile<T>(rppPath);
                    isLoaded = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[Rust++] RPP Migration failed: {rppFile} {ex}");
                }
            }

            // If JSON exists or we successfully loaded data, delete all legacy files.
            if (File.Exists(jsonPath) || isLoaded)
            {
                if (File.Exists(xmlPath))
                {
                    File.Delete(xmlPath);
                    Logger.Log($"[Rust++] Deleted legacy XML: {xmlFile}");
                }

                if (rppPath != null && File.Exists(rppPath))
                {
                    File.Delete(rppPath);
                    Logger.Log($"[Rust++] Deleted legacy RPP: {rppFile}");
                }

                // Ensure a JSON exists if we migrated
                if (!File.Exists(jsonPath) && isLoaded)
                    SaveJson(jsonFile, data);
            }

            return data;
        }

        public static T ObjectFromXML<T>(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(path))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static T ObjectFromFile<T>(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}