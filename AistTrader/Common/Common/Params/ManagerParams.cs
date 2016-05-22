using System;
using StockSharp.BusinessEntities;

namespace Common.Params
{
    [Serializable]
    public class ManagerParams
    {
        public ManagerParams(){}
        public ManagerParams(Common.Entities.Portfolio portfolio, string agent, Security security)
        {
            Portfolio = portfolio;
            AgentOrGroup = agent;
            Position = "-1";
            Transaction = -1;
            FinalTransaction = -1;
            Tool = security;
        }
        public Common.Entities.Portfolio Portfolio { get; set; }
        public string AgentOrGroup { get; set; }
        public Security Tool { get; set; }
        public string Position { get; set; }
        public int Transaction { get; set; }
        public double FinalTransaction { get; set; }
        public OperationCommand Command { get; set; }
    }
}
