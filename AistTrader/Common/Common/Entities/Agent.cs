using System;
using System.Xml;
using System.Xml.Serialization;
using Common.Settings;

namespace Common.Entities
{
    [Serializable]
    public class Agent : ICloneable
    {
        public Agent() { }
        
        public Agent(string name, AlgorithmSettings algorithm)
        {
            Name = name;
            _Agent = algorithm;
        }


        // ReSharper disable MemberCanBePrivate.Global
        public string Name { get; set; }
        [XmlArray("Agents")]
        public AlgorithmSettings _Agent { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        //public decimal GetStrategyVolume(Security security, Portfolio portfolio = null)
        //{
        //    if (portfolio == null)
        //    {
        //        portfolio = security.Trader.Portfolios.FirstOrDefault(p => p.Name == Name);
        //    }

        //    if (portfolio == null) return 1;

        //    var margin = security.MarginBuy != 0 ? security.MarginBuy : 8000;
        //    //var volume = Strategy.Volume.Type == UnitTypes.Absolute
        //    //                ? Strategy.Volume.Value
        //    //                : Math.Round(Strategy.Volume.Value * portfolio.BeginValue / (100 * margin));

        //    return volume == 0 ? 1 : volume;
        //}

        public object Clone()
        {
            Agent temp = (Agent)this.MemberwiseClone();
            temp._Agent = new AlgorithmSettings
            {
                GroupName = this._Agent.GroupName,
                AgentName = this._Agent.AgentName,
                SettingsStorage = this._Agent.SettingsStorage,
                Algorithm = this._Agent.Algorithm,
                ConnectionCount = this._Agent.ConnectionCount,
                Contracts = this._Agent.Contracts,
                IsChecked = this._Agent.IsChecked
            };
            return temp;
        }
    }
}
