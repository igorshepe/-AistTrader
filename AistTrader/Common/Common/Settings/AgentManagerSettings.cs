using System;
using Common.Entities;
using StockSharp.BusinessEntities;

namespace Common.Settings
{
    [Serializable]
    public class AgentManagerSettings
    {
        public AgentManagerSettings()
        {
        }
        public AgentManagerSettings(AgentPortfolio portfolio, string agent, Security security)
        {
            Account = portfolio;
            AgentOrGroup = agent;
            Position = "-1";
            Transaction = -1;
            FinalTransaction = -1;
            Tool = security;
        }
        public AgentPortfolio Account { get; set; }
        public string AgentOrGroup { get; set; }
        //TODO: уточнить типы данных
        public Security Tool { get; set; }
        public string Position { get; set; }
        public int Transaction { get; set; }
        public double FinalTransaction { get; set; }
    }
}
