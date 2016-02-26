using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;
using StockSharp.Messages;
using Strategies.Common;

namespace Common.Settings
{
    [Serializable]
    public class AgentSettings
    {
        AgentSettings() { }

        public AgentSettings(string type, Unit volume, bool isEnabled, string secName, SerializableDictionary<string, object> settingsStorage, bool hasSettings, string agentName)
        {
            Type = type;
            Volume = volume ?? new Unit(1);
            IsEnabled = isEnabled;
            SettingsStorage = settingsStorage;
            SecurityName = secName;
            HasSettings = hasSettings;
            AgentName = agentName;
        }

        // ReSharper disable MemberCanBePrivate.Global
        public string AgentName { get; set; }
        public Unit Volume { get; set; }
        public string Type { get; set; }
        public SerializableDictionary<string, object> SettingsStorage { get; set; }
        public bool IsEnabled { get; set; }
        public string SecurityName { get; set; }
        public bool HasSettings { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        [XmlIgnore]
        public bool IsRunning { get; set; }

        public override string ToString()
        {
            return AgentName.ToString(CultureInfo.InvariantCulture);
        }
    }
    [TypeConverter]
    public enum PickedStrategy
    {
        StrategyScript,
        StrategyNoParam,
        AistInvestStrategy,
        ChStrategy
        //, NewStrategy
    }

    public enum AgentWorkMode
    {
        Single,
        Group
    }
}
