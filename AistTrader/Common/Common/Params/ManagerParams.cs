using System;
using System.Xml.Serialization;
using StockSharp.BusinessEntities;

namespace Common.Params
{
    [Serializable]
    public class ManagerParams
    {
        public ManagerParams(){}
        public ManagerParams(Common.Entities.Portfolio portfolio, string agent, string security)
        {
            Portfolio = portfolio;
            AgentOrGroup = agent;
            Position = "-1";
            Transaction = -1;
            FinalTransaction = -1;
            Tool = security;
            AgentMangerCurrentStatus = AgentManagerStatus.Stopped;
            IsChecked = true;
            //Command = AgentManagerOperationCommand.Start;
        }

        public bool IsConnected { get; set; }
        public AgentManagerStatus AgentMangerCurrentStatus { get; set; }
        public Common.Entities.Portfolio Portfolio { get; set; }
        public string AgentOrGroup { get; set; }
        //[XmlIgnore]
        public string Tool { get; set; }
        public string Position { get; set; }
        public int Transaction { get; set; }
        public double FinalTransaction { get; set; }
        public AgentManagerOperationCommand Command { get; set; }
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
    }
}
