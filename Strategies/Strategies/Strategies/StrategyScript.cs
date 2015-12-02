using Ecng.Common;
using StockSharp.Algo.Strategies;
using StockSharp.Logging;
using Strategies.Common;
using Strategies.Settings;

namespace Strategies.Strategies
{
    public class StrategyScript : Strategy, IOptionalSettings
    {
        private decimal StopLong { get; set; }
        private decimal StopShort { get; set; }
        private SerializableDictionary<string, object> SettingsStorage { get; set; }

        public StrategyScript() { }
        public StrategyScript(SerializableDictionary<string, object> settingsStorage)
        {
            SettingsStorage = settingsStorage;
            UpdateStrategySettings();
        }

        public override void Start()
        {
            //Security.WhenNewTrades().Do(trades => this.AddInfoLog("New trades: {0}".Put(trades.First().Price)));
            base.Start();
        }

        private void UpdateStrategySettings()
        {
            string errorString = null;

            object obj;
            var res = SettingsStorage.TryGetValue(StrategyScriptDefaultSettings.StopLongString, out obj);
            if (!res)
            {
                errorString = string.Format("Ошибка получения стопа для лонга из настроек. Используем {0}", StrategyScriptDefaultSettings.StopLong);
            }
            StopLong = (decimal?)obj ?? StrategyScriptDefaultSettings.StopLong;


            res = SettingsStorage.TryGetValue(StrategyScriptDefaultSettings.StopShortString, out obj);
            if (!res)
            {
                errorString = string.Format("{0}\r\nОшибка получения стопа для шорта из настроек. Используем {1}", errorString, StrategyScriptDefaultSettings.StopShort);
            }
            StopShort = (decimal?)obj ?? StrategyScriptDefaultSettings.StopShort;

            if (errorString != null) this.AddWarningLog(errorString);
        }

        public string GetFriendlyName()
        {
            return "StrategyScript {0} - {1}".Put(StopLong, StopShort);
        }
    }
}
