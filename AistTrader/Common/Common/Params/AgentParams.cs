﻿using System;
using Strategies.Common;

namespace Common.Params
{
    [Serializable]
    public class AgentParams
    {
        public AgentParams() { }
        public AgentParams(string fName, int connectionCount, int contracts, SerializableDictionary<string, object> settingsStorage, string agentName,string compiledName,string toolTipName, AgentPhantomParams phantomParams)
        {
            FriendlyName = fName;
            ConnectionCount = connectionCount;
            SettingsStorage = settingsStorage;
            Contracts = contracts;
            AgentName = agentName;
            GroupName = "ungrouped agents";
            AgentCompiledName = compiledName;
            ToolTipName = toolTipName;
            PhantomParams = phantomParams;
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
        public AgentPhantomParams PhantomParams { get; set; }
    }
    public enum AgentWorkMode
    {
        Single,
        Group
    }
    [Serializable]
    public class AgentPhantomParams
    {
        public AgentPhantomParams() { }
        public AgentPhantomParams(string agentName, string groupName, string amount)
        {
            AgentName = agentName;
            GroupName = groupName;
            Amount = amount;
        }
        public string AgentName { get; set; }
        public string GroupName { get; set; }
        public string Amount { get; set; }
    }

}
