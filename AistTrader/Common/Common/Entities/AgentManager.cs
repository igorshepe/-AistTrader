using System;
using Common.Params;
using StockSharp.BusinessEntities;

namespace Common.Entities
{
    [Serializable]
    public class AgentManager
    {
        AgentManager() { }
        public AgentManager(string name, ManagerParams agentManager, Security tool, string amount)
        {
            Name = name;
            AgentManagerSettings = agentManager;
            Tool = tool;
            Amount = amount;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; set; }
        public StockSharp.BusinessEntities.Security Tool { get; set; }
        //public Unit Amount { get; set; }
        public string Amount { get; set; }
        public ManagerParams AgentManagerSettings { get; set; }
    }
}
