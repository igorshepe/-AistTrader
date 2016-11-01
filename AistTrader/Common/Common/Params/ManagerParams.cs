using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.Params
{
    [DataContract(Namespace = "")]
    public class ManagerParams
    {
        public ManagerParams() { }

        public ManagerParams(Entities.Portfolio portfolio, string agent, string security)
        {
            Portfolio = portfolio;
            AgentOrGroup = agent;
            Position = 0;
            DayMargin = 0;
            TotalMargin = 0;
            Tool = security;
            AgentMangerCurrentStatus = AgentManagerStatus.Stopped;
            IsChecked = true;
            CurrentMargin = 0;
            TradeEntryPrice = 0;
            СurrentPrice = 0;
        }

        [DataMember()]
        public bool IsConnected { get; set; }
        [DataMember()]
        public AgentManagerStatus AgentMangerCurrentStatus { get; set; }
        [DataMember()]
        public Common.Entities.Portfolio Portfolio { get; set; }
        [DataMember()]
        public string AgentOrGroup { get; set; }
        [DataMember()]
        public string Tool { get; set; }
        [DataMember()]
        public int Position { get; set; }
        [DataMember()]
        public decimal DayMargin { get; set; }
        [DataMember()]
        public decimal TotalMargin { get; set; }
        [DataMember()]
        public List<decimal> TotalMarginList { get; set; }

        [DataMember()]
        public decimal CurrentMargin { get; set; }
        [DataMember()]
        public decimal TradeEntryPrice { get; set; }
        [DataMember()]
        public  decimal СurrentPrice { get; set; }
        [DataMember()]
        public AgentManagerOperationCommand Command { get; set; }
        [DataMember()]
        public bool IsChecked { get; set; }

        public enum AgentManagerStatus
        {
            Running,
            Stopped,
            Starting,
            Stopping
        }

        public enum AgentManagerOperationCommand
        {
            Start,
            Stop
        };

        public enum AgentManagerDeleteMode
        {
            ClosePositionsAndDelete,
            WaitForClosingAndDeleteAfter
        }
    }
}
