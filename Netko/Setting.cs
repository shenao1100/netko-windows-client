using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.CompilerServices;

namespace Netko
{
    public class AccountStruct
    {
        public string? cookie { get; set; }
        public string? token { get; set; }
    }

    public class SettingJsonStruct
    {
        public string? download_path { get; set; }
        public string? theme { get; set; }
        public List<AccountStruct>? account { get; set; }

        public Dictionary<string, string>? custom_settings { get; set; }
    }

    public class MeowSetting
    {
        private static FileStream ConfigFile { get; set; }
        static MeowSetting()
        {
            ConfigFile = new FileStream("config.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public static SettingJsonStruct NetkoConfig = new SettingJsonStruct();
        public static List<AccountStruct> Account { get; set; }
        private static Dictionary<string, string> CustomSetting = new Dictionary<string, string>();

        
        public static void SaveConfig()
        {
            // clear config file
            ConfigFile.SetLength(0);
            // set pointer
            ConfigFile.Position = 0;
            string jsonString = JsonConvert.SerializeObject(NetkoConfig, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(ConfigFile, Encoding.UTF8, leaveOpen:true))
            {
                writer.Write(jsonString);
            }

            
        }
        public static void SetAllDefault(bool clear_account=false)
        {
            CustomSetting = new Dictionary<string, string>();
            NetkoConfig.custom_settings = CustomSetting;
            NetkoConfig.theme = "Default";
            NetkoConfig.download_path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            if (clear_account)
            {
                NetkoConfig.account = new List<AccountStruct>();
            }

        }
        public static void LoadConfig()
        {
            string string_conttent;
            using (StreamReader reader = new StreamReader(ConfigFile, Encoding.UTF8, leaveOpen:true))
            {
                string_conttent = reader.ReadToEnd();
            }
            if (string_conttent == string.Empty)
            {
                SetAllDefault(true);
                SaveConfig();
                return;
            }
            SettingJsonStruct? serialize_obj = JsonConvert.DeserializeObject<SettingJsonStruct>(string_conttent);
            if (serialize_obj != null)
            {
                Account = serialize_obj.account;
                NetkoConfig = serialize_obj;
                CustomSetting = serialize_obj.custom_settings;
            }
        }
        public static string GetDownloadPath()
        {
            if (NetkoConfig.download_path == null)
            {
                SetAllDefault();
                SaveConfig();
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            }
            else
            {
                return NetkoConfig.download_path;
            }
        }
        public static string GetTheme()
        {
            if (NetkoConfig.theme == null)
            {
                SetAllDefault();
                SaveConfig();
                return "Default";
            }
            else
            {
                return NetkoConfig.theme;
            }
        }
        public static void SetDownloadPath(string path)
        {
            NetkoConfig.download_path = path;
            SaveConfig();
        }
        public static void SetTheme(string theme)
        {
            NetkoConfig.theme = theme;
            SaveConfig();
        }

        public static List<AccountStruct> GetAllAccount()
        {
            if (Account == null)
            {
                return new List<AccountStruct>();
            }
            return Account;
        }
        public static void InsertAccount(string cookie, string token)
        {
            foreach (var account in GetAllAccount())
            {
                if (account.token == token)
                {
                    return;
                }
            }
            Account.Add(new AccountStruct
            {
                cookie = cookie,
                token = token
            });
            SaveConfig();
        }
        public static void RemoveAccount(string token)
        {
            foreach (var account in GetAllAccount())
            {
                if (token == account.token)
                {
                    Account.Remove(account);
                    break;
                }
            }
            SaveConfig();
        }
        public static string? GetCustomSetting(string key, string? default_value=null)
        {
            if (CustomSetting.ContainsKey(key))
            {
                return CustomSetting[key];
            }
            return default_value;
        }
    }
}
