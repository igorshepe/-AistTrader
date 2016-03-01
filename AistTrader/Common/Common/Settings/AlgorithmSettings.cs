using System;
using Strategies.Common;

namespace Common.Settings
{
    [Serializable]
    public class AgentParams
    {
        public AgentParams() { }
        public AgentParams(string fName, int connectionCount, bool isChecked, int contracts, SerializableDictionary<string, object> settingsStorage, string agentName, decimal amount)
        {
            FriendlyName = fName;
            ConnectionCount = connectionCount;
            IsChecked = isChecked;
            SettingsStorage = settingsStorage;
            Contracts = contracts;
            AgentName = agentName;
            GroupName = "ungrouped agents";
            Amount = amount ;
        }

        public string AgentName { get; set; }
        public int ConnectionCount { get; set; }
        public string FriendlyName { get; set; }
        public SerializableDictionary<string, object> SettingsStorage { get; set; }
        public bool IsChecked { get; set; }
        public int Contracts { get; set; }
        public string GroupName { get; set; }
        //todo : реализовать сериализацию данного параметра
        //public Unit Amount { get; set; }
        public decimal Amount { get; set; }
    }
}
