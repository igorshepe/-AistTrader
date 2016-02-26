using System;
using Common.Settings;
using StockSharp.BusinessEntities;
using StockSharp.Messages;

namespace Common.Entities
{
    [Serializable]
    public class AgentManager
    {
        AgentManager() { }
        public AgentManager(string name, AgentManagerSettings agentManager, Security tool, int amount)
        {
            Name = name;
            AgentManagerSettings = agentManager;
            Tool = tool;
            Amount = amount;
        }
        public string Name { get; set; }
        public StockSharp.BusinessEntities.Security Tool { get; set; }
        //public Unit Amount { get; set; }
        public int Amount { get; set; }
        public AgentManagerSettings AgentManagerSettings { get; set; }
    }
}
