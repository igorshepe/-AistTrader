using System;
using System.Runtime.Serialization;
using Common.Params;

namespace Common.Entities
{
    [DataContract(Namespace = "")]
    public class Agent : ICloneable
    {
        public Agent() { }
        public Agent(string name, AgentParams agentParams)
        {
            Name = name;
            Params = agentParams;
        }
        [DataMember()]
        public string Name { get; set; }
        [DataMember()]
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
                IsChecked = Params.IsChecked,
                PhantomParams = Params.PhantomParams
            };
            return temp;
        }
    }
}
