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
            Transaction = -1;
            FinalTransaction = -1;
            Tool = security;
            AgentMangerCurrentStatus = AgentManagerStatus.Stopped;
            IsChecked = true;
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
        public int Transaction { get; set; }
        [DataMember()]
        public double FinalTransaction { get; set; }
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
