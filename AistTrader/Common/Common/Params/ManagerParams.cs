using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Common.Params
{
    [DataContract(Namespace = "")]
    public class ManagerParams : INotifyPropertyChanged
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
        public List<decimal> TotalMarginList { get; set; }
        private decimal _totalMargin;

        private decimal _currenMargin;
        [DataMember()]
        public decimal TradeEntryPrice { get; set; }

        private decimal _currentPrice;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember()]
        public decimal СurrentPrice
        {
            get { return _currentPrice; }
            set
            {
                _currentPrice = value;
                OnPropertyChanged("СurrentPrice");
            }
        }

        [DataMember()]
        public decimal CurrentMargin
        {
            get { return _currenMargin; }
            set
            {
                _currenMargin = value;
                OnPropertyChanged("CurrentMargin");
            }
        }

        [DataMember()]
        public decimal TotalMargin {
            get {return _totalMargin; }
            set {
                _totalMargin = value;
                OnPropertyChanged("TotalMargin");

            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
