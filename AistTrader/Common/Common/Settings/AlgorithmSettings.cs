using System;
using StockSharp.Messages;
using Strategies.Common;

namespace Common.Settings
{
    [Serializable]
    public class AlgorithmSettings
    {
        public AlgorithmSettings() { }
        public AlgorithmSettings(PickedStrategy algorithm, int connectionCount, bool isChecked, int contracts, SerializableDictionary<string, object> settingsStorage, string agentName, decimal amount)
        {
            Algorithm = algorithm;
            ConnectionCount = connectionCount;
            IsChecked = isChecked;
            SettingsStorage = settingsStorage;
            Contracts = contracts;
            AgentName = agentName;
            GroupName = "Without Group";
            Amount = amount ;
        }

        public string AgentName { get; set; }
        public int ConnectionCount { get; set; }
        public PickedStrategy Algorithm { get; set; }
        public SerializableDictionary<string, object> SettingsStorage { get; set; }
        public bool IsChecked { get; set; }
        public int Contracts { get; set; }
        public string GroupName { get; set; }
        //public Unit Amount { get; set; }
        public decimal Amount { get; set; }
    }
}
