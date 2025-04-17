using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppScheduleOne.Economy;
using UnityEngine;

namespace AutoCounter3
{
    public class ConfigData
    {
        public EDealWindow DealWindow { get; set; } = EDealWindow.LateNight;
        public int AutoCounterInterval { get; set; } = 5; // Default to 5 seconds set to -1 to disable
        public float? PricePerUnit { get; set; } = null; // Optional, null means calculate dynamically
        public int RoundTo { get; set; } = 5; // Optional, default is 5
        public bool EnableCounter { get; set; } = true; // Optional, default is true
        public KeyCode Hotkey { get; set; } = KeyCode.F4; // Optional, default is F4
        public bool choosetimemanual { get; set; } = false; // Optional, default is false
    }
}
