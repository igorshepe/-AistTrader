using System;
using System.Linq;
using Ecng.Common;
using Ecng.ComponentModel;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Logging;
using Strategies.Common;
using Strategies.Settings;

namespace Strategies.Strategies
{
    public class AistInvestStrategy : BaseStrategy, IOptionalSettings
    {
        private DateTime _lastTimeFrame = DateTime.MinValue;
        private readonly CandleSeries _candleSeries;
        private decimal StopLong { get; set; }
        private decimal StopShort { get; set; }

        public AistInvestStrategy() { }

        public AistInvestStrategy(SerializableDictionary<string, object> settingsStorage)
            : base(settingsStorage)
        {
            UpdateStrategySettings();
        }
        public AistInvestStrategy(SerializableDictionary<string, object> settingsStorage, CandleSeries candleSeries)
            : base(settingsStorage, (TimeSpan)candleSeries.Arg)
        {
            _candleSeries = candleSeries;
            UpdateStrategySettings();
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

        public override string GetFriendlyName()
        {
            return "AistInvestStrategy {0} - {1}".Put(StopLong, StopShort);
        }

        public string GetParamsForFriendlyName()
        {
            return StopLong.GetDisplayName();
        }

        protected override ProcessResults OnProcess()
        {
            var currentPosition = Position;
            if (TotalWorkingTime < WaitTime)
            {
                currentPosition = GetCurrentPosition();

                if (Position != currentPosition)
                {
                    this.AddInfoLog("Позиция стратегии ({0}) не совпадает с текущей позицией ({1}). Меняем.".Put(Position, currentPosition));
                    Position = currentPosition;
                }
            }

            var lastWorkingTime = Security.Board.WorkingTime.Periods.Last();
            //if (!lastWorkingTime.HasMaxValue)
            //{
            //    this.AddErrorLog("Не удалось получить рабочее время биржи");
            //    return ProcessResults.Stop;
            //}

            //var roundMarketTime = TimeFrame.GetCandleTime(Security);

            //if (TotalWorkingTime > WaitTime &&
            //    (ProcessState == ProcessStates.Stopping
            //    ||
            //    currentPosition == 0 && roundMarketTime.TimeOfDay > lastWorkingTime.Max && Security.State == SecurityStates.Trading
            //    ))
            //{
            //    CancelAllOrders();
            //    return ProcessResults.Stop;
            //}

            if (currentPosition != 0 && !CheckStop(currentPosition))
            {
                //var neededTime = Connector.GetLogLevel().Date.Add(lastWorkingTime.Max).AddMinutes(-15);
                //var neededCandle = _candleSeries.GetTimeFrameCandle(neededTime);
                //if (neededCandle == null)
                //{
                //    this.AddInfoLog("Свечки не содержат нужное для установки стопа время - ждём формирования...");
                //}
                //else
                //{
                //    this.AddInfoLog("Удалось получить свечку за {0}. Цена закрытия {1}.", neededCandle.OpenTime, neededCandle.ClosePrice);
                //    if (currentPosition > 0)
                //    {
                //        PlaceStrategyStop(Sides.Sell, neededCandle.ClosePrice - StopLong, Math.Abs(currentPosition));
                //    }
                //    else
                //    {
                //        PlaceStrategyStop(Sides.Buy, neededCandle.ClosePrice + StopShort, Math.Abs(currentPosition));
                //    }
                //}
            }

            //if (_lastTimeFrame.CompareTo(roundMarketTime) != 0 && Security.ExchangeBoard.IsTradeTime(Trader))
            //{
            //    this.AddInfoLog("Новая 5-минутка: {0}", roundMarketTime);
            //    this.AddInfoLog("Текущая позиция: {0}", currentPosition);

            //    if (currentPosition == 0)
            //    {
            //        var firstWorkingTime = Security.ExchangeBoard.WorkingTime.Times.First();
            //        if (!firstWorkingTime.HasMinValue)
            //        {
            //            this.AddErrorLog("Не удалось получить рабочее время биржи");
            //            return ProcessResults.Stop;
            //        }

            //        if (roundMarketTime.TimeOfDay == firstWorkingTime.Min.Add(new TimeSpan(0, 5, 0)))
            //        {
            //            this.AddInfoLog("Наступило открытие рынка - проверяем");

            //            var lastCandle = _candleSeries.GetTimeFrameCandle(roundMarketTime);
            //            if (lastCandle == null)
            //            {
            //                this.AddInfoLog("Свечки не содержат текущее время - ждём формирования...");
            //                return ProcessResults.Continue;
            //            }

            //            //Получили все данные для текущей свечки - больше её не анализируем
            //            _lastTimeFrame = roundMarketTime;


            

            //var firstTime = Connector.GetMarketTime(Security.ExchangeBoard.Exchange).Date.Add(firstWorkingTime.Min);

            //            var oldCandles =
            //                _candleSeries.GetCandles<TimeFrameCandle>(new Range<DateTime>(firstTime,
            //                                                                              Connector.GetMarketTime(Security.ExchangeBoard.Exchange)))
            //                             .ToList();
            //            if (oldCandles.Count < 2)
            //            {
            //                this.AddInfoLog("Не удалось получить предыдущие свечки...");
            //                return ProcessResults.Continue;
            //            }

            //            var prevCandle = oldCandles.ElementAt(oldCandles.Count - 2);
            //            this.AddInfoLog("prevCandle.Open: {0}, prevCandle.Close: {1}", prevCandle.OpenPrice,
            //                            prevCandle.ClosePrice);

            //            if (prevCandle.ClosePrice > prevCandle.OpenPrice)
            //            {
            //                //UpdateStrategyVolume(); TODO
            //                BuyStrategyVolumeAtMarket();
            //                PlaceStrategyStop(Sides.Sell, prevCandle.ClosePrice - StopLong);
            //            }
            //            else if (prevCandle.ClosePrice < prevCandle.OpenPrice)
            //            {
            //                //UpdateStrategyVolume(); TODO
            //                SellStrategyVolumeAtMarket();
            //                PlaceStrategyStop(Sides.Buy, prevCandle.ClosePrice + StopShort);
            //            }
            //            else
            //            {
            //                this.AddInfoLog("Входа сегодня нет.");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        _lastTimeFrame = roundMarketTime;
            //    }
            //}

            return ProcessResults.Continue;
        }
    }
}
