using System.IO;
using Il2CppScheduleOne.Economy;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace AutoCounter3
{
    public class ConfigManager
    {
        private const string ConfigFilePath = "UserData/AutoCounterConfig.cfg";

        public ConfigData Config { get; private set; }

        public ConfigManager()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                Config = JsonConvert.DeserializeObject<ConfigData>(json) ?? new ConfigData();
            }
            else
            {
                Config = new ConfigData();
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
        }
    }
}
