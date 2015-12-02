using System;
using Common.Settings;

namespace Common.Entities
{
    //ToDO: переименовать соотвественно
    [Serializable]
    public class AgentConnection
    {
        AgentConnection() { }

        public AgentConnection(string name, ConnectionsSettings connectionSettings)
        {
            Name = name;
            Connection = connectionSettings;
        }

        // ReSharper disable MemberCanBePrivate.Global
        public string Name { get; set; }
        //public TerminalSettings Terminal { get; set; }
        public ConnectionsSettings Connection { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        //public decimal GetStrategyVolume(Security security, Portfolio portfolio = null)
        //{
        //    if (portfolio == null)
        //    {
        //        portfolio = security.Trader.Portfolios.FirstOrDefault(p => p.Name == Name);
        //    }

        //    if (portfolio == null) return 1;

        //    var margin = security.MarginBuy != 0 ? security.MarginBuy : 8000;
        //    var volume = Agent.Volume.Type == UnitTypes.Absolute
        //                    ? Agent.Volume.Value
        //                    : Math.Round(Agent.Volume.Value * portfolio.BeginValue / (100 * margin));

        //    return volume == 0 ? 1 : volume;
        //}
    }
}
