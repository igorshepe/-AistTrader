using System;
using System.Xml.Serialization;
using Common.Params;
using StockSharp.BusinessEntities;

namespace Common.Entities
{
    [Serializable]
    public class AgentManager
    {
        AgentManager() { }
        public AgentManager(string name, ManagerParams agentManager, Security tool, string amount, string alias)
        {
            AgentManagerUniqueId = alias;
            Name = name;
            Alias = alias;
            AgentManagerSettings = agentManager;
            Tool = tool;
            Amount = amount;
        }
        public override string ToString()
        {
            return AgentManagerUniqueId;
        }
        public string AgentManagerUniqueId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        [XmlIgnore]
        public StockSharp.BusinessEntities.Security Tool { get; set; }
        //todo по аналогии дописать конвертер как в группах агетов
        public string Amount { get; set; }
        public ManagerParams AgentManagerSettings { get; set; }
        public TradeParams TradeParams { get; set; }
    }

    public class TradeParams
    {
        public bool DirectIns { get; set; }
        public bool DirectOuts { get; set; }
        public int AutoOpenBarAction { get; set; }
        public int AutoCloseBarAction { get; set; }
        public bool IgnorePositionOutOfHistory { get; set; }
        public bool NotifyOnMissedIns { get; set; }
        public bool NotOpenIfGap { get; set; }
        public bool NotifyOnRecount { get; set; }
        public int VirtualCandleMax { get; set; }
        public int WaitOnSuccessfulOut { get; set; }
        public int WaitOnSuccessfulIn { get; set; }
        public bool SimulatePositionSequence { get; set; }
        public int SlippingInSteps { get; set; }
        public int SlippingInPercent { get; set; }
        public bool TakeProfitWithNoSlipping { get; set; }
        public bool OpeningWithLimitingOrders { get; set; }
        public bool ByMarketWithFixedPrice { get; set; }
        public bool BadOrdersByMarket { get; set; }
    }
}

