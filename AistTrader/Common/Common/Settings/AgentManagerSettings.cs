using System;
using Common.Entities;

namespace Common.Settings
{
    [Serializable]
    public class AgentManagerSettings
    {
        public AgentManagerSettings()
        {
        }
        public AgentManagerSettings(AgentPortfolio portfolio, string agent)
        {
            Account = portfolio;
            AgentOrGroup = agent;
            Tool = "-1";
            Position = "-1";
            Transaction = -1;
            FinalTransaction = -1;
        }
        public AgentPortfolio Account { get; set; }
        public string AgentOrGroup { get; set; }
        //TODO: уточнить типы данных
        public string Tool { get; set; }
        public string Position { get; set; }
        public int Transaction { get; set; }
        public double FinalTransaction { get; set; }
    }
}
