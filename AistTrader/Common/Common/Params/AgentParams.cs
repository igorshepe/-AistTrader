using System;
using Strategies.Common;

namespace Common.Params
{
    [Serializable]
    public class AgentParams
    {
        public AgentParams() { }
        public AgentParams(string fName, int connectionCount, int contracts, SerializableDictionary<string, object> settingsStorage, string agentName,string compiledName,string toolTipName)
        {
            FriendlyName = fName;
            ConnectionCount = connectionCount;
            SettingsStorage = settingsStorage;
            Contracts = contracts;
            AgentName = agentName;
            GroupName = "ungrouped agents";
            AgentCompiledName = compiledName;
            ToolTipName = toolTipName;
        }

        public string AgentName { get; set; }
        public string ToolTipName { get; set; }
        public string AgentCompiledName { get; set; }
        public int ConnectionCount { get; set; }
        public string FriendlyName { get; set; }
        public SerializableDictionary<string, object> SettingsStorage { get; set; }
        public bool IsChecked { get; set; }
        public int Contracts { get; set; }
        public string GroupName { get; set; }
        //todo : СПРОСИТЬ У SS ЧЕ ЗА ФИНТ С КОНСТРУТОРОМ
        public string Amount { get; set; }
    }
    public enum AgentWorkMode
    {
        Single,
        Group
    }
}
