using System;
using Strategies.Common;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StockSharp.BusinessEntities;

namespace Common.Params
{
    [DataContract(Namespace = "")]
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
            TransactionIDs = new List<long>( );
        }

        [DataMember()]
        public string Security { get; set; }
        [DataMember()]
        public List<long> TransactionIDs { get; set; }
        [DataMember()]
        public string AgentName { get; set; }
        [DataMember()]
        public string ToolTipName { get; set; }
        [DataMember()]
        public string AgentCompiledName { get; set; }
        [DataMember()]
        public int ConnectionCount { get; set; }
        [DataMember()]
        public string FriendlyName { get; set; }
        [DataMember()]
        public SerializableDictionary<string, object> SettingsStorage { get; set; }
        [DataMember()]
        public bool IsChecked { get; set; }
        [DataMember()]
        public int Contracts { get; set; }
        [DataMember()]
        public string GroupName { get; set; }
        [DataMember()]
        //todo : СПРОСИТЬ У SS ЧЕ ЗА ФИНТ С КОНСТРУТОРОМ
        public string Amount { get; set; }
        [DataMember()]
        public AgentPhantomParams PhantomParams { get; set; }
        [DataMember()]
        public List<long> TransactionId {get;set;}
        [DataMember()]
        public Security Tool { get; set; }
    }

    public enum AgentWorkMode
    {
        Single,
        Group
    }
    
    [DataContract(Namespace = "")]
    public class AgentPhantomParams
    {
        public AgentPhantomParams() { }

        public AgentPhantomParams(string agentName, string groupName, string amount)
        {
            AgentName = agentName;
            GroupName = groupName;
            Amount = amount;
        }

        [DataMember()]
        public string AgentName { get; set; }
        [DataMember()]
        public string GroupName { get; set; }
        [DataMember()]
        public string Amount { get; set; }
    }
}
