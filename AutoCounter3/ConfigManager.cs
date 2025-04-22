using System.IO;
using Newtonsoft.Json;

namespace AutoCounter3
{
    public class ConfigManager
    {
        private const string OldConfigFilePath = "UserData/AutoCounterConfig.cfg";
        // Using new config name because for some reason Mod Manager wants the the Config Category to start with or equal the mod name
        private const string ConfigName = "AutoAcceptCounters";
        private const string ConfigFilePath = "UserData/" + ConfigName + ".cfg";

        public BaseConfigData Config { get; private set; }

        public ConfigManager()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(OldConfigFilePath)) // Convert old json config to the new one
            {
                try
                {
                    var json = File.ReadAllText(OldConfigFilePath);
                    Config = JsonConvert.DeserializeObject<JsonConfigData>(json);
                    if (Config != null)
                        File.Delete(OldConfigFilePath);
                }
                catch
                {
                    // Config is already in TOML format
                }
            }

            Config = new MelonConfigData(ConfigName, ConfigFilePath, Config as JsonConfigData);

            SaveConfig();
        }

        public void SaveConfig()
        {
            Config.Save();
        }
    }
}
