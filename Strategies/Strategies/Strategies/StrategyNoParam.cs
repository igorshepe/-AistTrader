using System;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Logging;
using Strategies.Common;

namespace Strategies.Strategies
{
    public class StrategyNoParam : BaseStrategy
    {
        public StrategyNoParam() { }
        public StrategyNoParam(SerializableDictionary<string, object> settingsStorage, CandleSeries candleSeries)
            : base(settingsStorage, (TimeSpan)candleSeries.Arg)
        {
            SettingsStorage = settingsStorage;
        }

        protected override ProcessResults OnProcess()
        {
            this.AddInfoLog("OnProcess вызван");
            return ProcessResults.Continue;
        }

        public override string GetFriendlyName()
        {
            return "StrategyNoParam";
        }

    }
}
