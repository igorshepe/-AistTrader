using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Common.Params;
using StockSharp.BusinessEntities;

namespace Common.Entities
{
    [DataContract(Namespace = "")]
    public class AgentManager
    {
        AgentManager() { }

        public AgentManager(string name, ManagerParams agentManager, string tool, string amount, string alias, List<StrategyInGroup> strategyInGroup)// List<long> transactionIdHistory//)
        {
            AgentManagerUniqueId = alias;
            Name = name;
            Alias = alias;
            AgentManagerSettings = agentManager;
            Tool = tool;
            Amount = amount;
            StrategyInGroup = strategyInGroup;
        }
        

        public override string ToString()
        {
            return AgentManagerUniqueId;
        }

        [DataMember()]
        public string AgentManagerUniqueId { get; set; }
        [DataMember()]
        public string Name { get; set; }
        [DataMember()]
        public string Alias { get; set; }
        [DataMember()]
        public string Tool { get; set; }
        [DataMember()]
        //todo по аналогии дописать конвертер как в группах агетов
        public string Amount { get; set; }
        [DataMember()]
        public ManagerParams AgentManagerSettings { get; set; }
        [DataMember()]
        public TradeParams TradeParams { get; set; }
        [DataMember()]
        public List<long> SingleAgentHistory { get; set; }
        [DataMember()]
        public int SingleAgentPosition { get; set; }

        [DataMember()]
        public List<MyTrade> SingleMyTradesHistory { get; set; }

        [DataMember()]
        public  List<StrategyInGroup> StrategyInGroup { get; set; }

        [DataMember()]
        public StrategyCloseState CloseState { get; set; }
    }

    [DataContract(Namespace = "")]
    public class StrategyInGroup
    {
        public StrategyInGroup()
        {
        }

        public StrategyInGroup(string name, List<long> transactionIdHistory, int position)
        {
            Name = name;
            TransactionIdHistory = transactionIdHistory;
            Position = position;
        }

        [DataMember()]
        public string Name { get; set; }

        [DataMember()]
        public List<long> TransactionIdHistory { get; set; }

        [DataMember()]
        public int Position { get; set; }

        [DataMember()]
        public List<MyTrade> MyTradesHistory { get; set; }

        [DataMember()]
        public StrategyCloseState CloseState { get; set; }
    }

    [DataContract(Namespace = "")]
    public class TradeParams
    {
        [DataMember()]
        public bool DirectIns { get; set; }
        [DataMember()]
        public bool DirectOuts { get; set; }
        [DataMember()]
        public int AutoOpenBarAction { get; set; }
        [DataMember()]
        public int AutoCloseBarAction { get; set; }
        [DataMember()]
        public bool IgnorePositionOutOfHistory { get; set; }
        [DataMember()]
        public bool NotifyOnMissedIns { get; set; }
        [DataMember()]
        public bool NotOpenIfGap { get; set; }
        [DataMember()]
        public bool NotifyOnRecount { get; set; }
        [DataMember()]
        public int VirtualCandleMax { get; set; }
        [DataMember()]
        public int WaitOnSuccessfulOut { get; set; }
        [DataMember()]
        public int WaitOnSuccessfulIn { get; set; }
        [DataMember()]
        public bool SimulatePositionSequence { get; set; }
        [DataMember()]
        public int SlippingInSteps { get; set; }
        [DataMember()]
        public int SlippingInPercent { get; set; }
        [DataMember()]
        public bool TakeProfitWithNoSlipping { get; set; }
        [DataMember()]
        public bool OpeningWithLimitingOrders { get; set; }
        [DataMember()]
        public bool ByMarketWithFixedPrice { get; set; }
        [DataMember()]
        public bool BadOrdersByMarket { get; set; }
    }
}

