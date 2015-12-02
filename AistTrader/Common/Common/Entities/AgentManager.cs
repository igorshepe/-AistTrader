using System;
using Common.Settings;
using StockSharp.Messages;

namespace Common.Entities
{
    [Serializable]
    public class AgentManager
    {
        AgentManager() { }
        public AgentManager(string name, AgentManagerSettings agentManager, string tool, Unit amount)
        {
            Name = name;
            AgentManagerSettings = agentManager;
            Tool = tool;
            Amount = amount;
        }
        public string Name { get; set; }
        public string Tool { get; set; }
        public Unit Amount { get; set; }
        public AgentManagerSettings AgentManagerSettings { get; set; }
    }
}
