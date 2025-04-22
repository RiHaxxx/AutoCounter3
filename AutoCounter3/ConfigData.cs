using Il2CppScheduleOne.Economy;
using MelonLoader;
using UnityEngine;

namespace AutoCounter3
{
    public abstract class BaseConfigData
    {
        public virtual EDealWindow DealWindow { get; set; } = EDealWindow.LateNight;
        public virtual int AutoCounterInterval { get; set; } = 5; // Default to 5 seconds set to -1 to disable
        public virtual float? PricePerUnit { get; set; } = null; // Optional, null means calculate dynamically
        public virtual int RoundTo { get; set; } = 5; // Optional, default is 5
        public virtual bool EnableCounter { get; set; } = true; // Optional, default is true
        public virtual KeyCode Hotkey { get; set; } = KeyCode.F4; // Optional, default is F4
        public virtual bool choosetimemanual { get; set; } = false; // Optional, default is false

        public virtual void Save() { }
    }

    public class JsonConfigData : BaseConfigData
    {

    }

    public class MelonConfigData : BaseConfigData
    {
        private MelonPreferences_Category config;
        private MelonPreferences_Entry<EDealWindow> configDealWindow;
        private MelonPreferences_Entry<int> configAutoCounterInterval;
        private MelonPreferences_Entry<float> configPricePerUnit;
        private MelonPreferences_Entry<int> configRoundTo;
        private MelonPreferences_Entry<bool> configEnableCounter;
        private MelonPreferences_Entry<KeyCode> configHotkey;
        private MelonPreferences_Entry<bool> configChooseTimeManual;

        public override EDealWindow DealWindow { get => configDealWindow.Value; set => configDealWindow.Value = value; }
        public override int AutoCounterInterval { get => configAutoCounterInterval.Value; set => configAutoCounterInterval.Value = value; }
        public override float? PricePerUnit { get => configPricePerUnit.Value < 0 ? (float?)null : configPricePerUnit.Value; set => configPricePerUnit.Value = value ?? -1; }
        public override int RoundTo { get => configRoundTo.Value; set => configRoundTo.Value = value; }
        public override bool EnableCounter { get => configEnableCounter.Value; set => configEnableCounter.Value = value; }
        public override KeyCode Hotkey { get => configHotkey.Value; set => configHotkey.Value = value; }
        public override bool choosetimemanual { get => configChooseTimeManual.Value; set => configChooseTimeManual.Value = value; }

        public MelonConfigData(string name, string filePath = null, JsonConfigData jsonConfigData = null)
        {
            config = MelonPreferences.CreateCategory(name);
            if (filePath != null)
                config.SetFilePath(filePath);

            configDealWindow = config.CreateEntry(
                identifier: "DealWindow",
                default_value: EDealWindow.LateNight,
                display_name: "Deal Window");

            configAutoCounterInterval = config.CreateEntry(
                identifier: "AutoCounterInterval",
                default_value: 5,
                display_name: "Auto Counter Interval");

            configPricePerUnit = config.CreateEntry(
                identifier: "PricePerUnit",
                default_value: -1f,
                display_name: "Price Per Unit");

            configRoundTo = config.CreateEntry(
                identifier: "RoundTo",
                default_value: 5,
                display_name: "Round To");

            configEnableCounter = config.CreateEntry(
                identifier: "EnableCounter",
                default_value: true,
                display_name: "Enable Counter");

            configHotkey = config.CreateEntry(
                identifier: "Hotkey",
                default_value: KeyCode.F4,
                display_name: "Hotkey");

            configChooseTimeManual = config.CreateEntry(
                identifier: "choosetimemanual",
                default_value: false,
                display_name: "Choose Time Manual");

            if (jsonConfigData != null)
            {
                DealWindow = jsonConfigData.DealWindow;
                AutoCounterInterval = jsonConfigData.AutoCounterInterval;
                PricePerUnit = jsonConfigData.PricePerUnit;
                RoundTo = jsonConfigData.RoundTo;
                EnableCounter = jsonConfigData.EnableCounter;
                Hotkey = jsonConfigData.Hotkey;
                choosetimemanual = jsonConfigData.choosetimemanual;
            }
        }

        public override void Save()
        {
            config.SaveToFile();
        }
    }
}
