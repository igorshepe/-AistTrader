using System;
using System.Xml.Serialization;
using Common.Params;

namespace Common.Entities
{
    [Serializable]
    public class Agent : ICloneable
    {
        public Agent() { }
        public Agent(string name, AgentParams agentParams)
        {
            Name = name;
            Params = agentParams;
        }
        public string Name { get; set; }
        public AgentParams Params { get; set; }

        public override string ToString()
        {
            return Name;
        }
        public object Clone()
        {
            Agent temp = (Agent)MemberwiseClone();
            temp.Params = new AgentParams
            {
                GroupName = Params.GroupName,
                AgentName = Params.AgentName,
                SettingsStorage = Params.SettingsStorage,
                FriendlyName = Params.FriendlyName,
                ConnectionCount = Params.ConnectionCount,
                Contracts = Params.Contracts,
                IsChecked = Params.IsChecked
            };
            return temp;
        }
    }
}
