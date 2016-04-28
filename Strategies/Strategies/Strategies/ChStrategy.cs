using System;
using System.Linq;
using Ecng.Common;
using Ecng.ComponentModel;
using NLog;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.Logging;
using StockSharp.Messages;
using Strategies.Common;
using Strategies.Settings;


namespace Strategies.Strategies
{
    public class ChStrategy : BaseStrategy, IOptionalSettings
    {
        private static readonly Logger TradesLogger = NLog.LogManager.GetLogger("TradesLogger");
        private CandleManager _candleManager;

        private CandleSeries _candleSeries;



        private readonly TimeSpan _timeFrame = TimeSpan.FromMinutes(5);


        private decimal FastSma { get; set; }
        private decimal SlowSma { get; set; }
        private decimal Period { get; set; }

        //settingsStorage
        public ChStrategy() { }

        public ChStrategy(SerializableDictionary<string, object> settingsStorage)
            : base(settingsStorage)
        {

            UpdateStrategySettings();

        }
        public ChStrategy(SerializableDictionary<string, object> settingsStorage, CandleSeries candleSeries)
            : base(settingsStorage, (TimeSpan)candleSeries.Arg)
        {

            UpdateStrategySettings();
        }

        private void UpdateStrategySettings()
        {
            string errorString = null;

            object obj;
            var res = SettingsStorage.TryGetValue(ChStrategyDefaultSettings.FastSmaString, out obj);
            if (!res)
            {
                errorString = string.Format("Ошибка получения значения FastSma из настроек. Используем {0}", ChStrategyDefaultSettings.FastSma);
            }
            FastSma = (decimal?)obj ?? ChStrategyDefaultSettings.FastSma;


            res = SettingsStorage.TryGetValue(ChStrategyDefaultSettings.SlowSmaString, out obj);
            if (!res)
            {
                errorString = string.Format("{0}\r\nОшибка получения значения SlowSma из настроек. Используем {1}", errorString, ChStrategyDefaultSettings.SlowSma);
            }
            SlowSma = (decimal?)obj ?? ChStrategyDefaultSettings.SlowSma;


            res = SettingsStorage.TryGetValue(ChStrategyDefaultSettings.PeriodString, out obj);
            if (!res)
            {
                errorString = string.Format("Ошибка получения значения FastSma из настроек. Используем {0}", ChStrategyDefaultSettings.Period);
            }
            Period = (decimal?)obj ?? ChStrategyDefaultSettings.Period;


            if (errorString != null) this.AddWarningLog(errorString);

        }

        // end settingsStorage



        public override string GetFriendlyName()
        {
            return "ChStrategy {0} - {1} - {2}".Put(FastSma, SlowSma, Period);
        }

        //=============================================================

        private bool NoActiveOrders { get { return Orders.Count(o => o.State == OrderStates.Active) == 0; } }

        public string GetParamsForFriendlyName()
        {
            return FastSma.GetDisplayName();
        }



        private void RunProcessGetCandles(TimeSpan timeFrame)
        {
            _candleManager = new CandleManager(Connector);

            var security = Security;

            _candleSeries = new CandleSeries(typeof(TimeFrameCandle), security, timeFrame);

            _candleManager.Start(_candleSeries);

        }



        protected override ProcessResults OnProcess()
        {
            // если наша стратегия в процессе остановки
            if (ProcessState == ProcessStates.Stopping)
            {
                // отменяем активные заявки
                CancelActiveOrders();

                // так как все активные заявки гарантированно были отменены, то возвращаем ProcessResults.Stop
                return ProcessResults.Stop;
            }


            RunProcessGetCandles(_timeFrame);




            TradesLogger.Info("Цикл стратегии - {0}", Connector.CurrentTime);




            return ProcessResults.Continue;
        }


    }
}

