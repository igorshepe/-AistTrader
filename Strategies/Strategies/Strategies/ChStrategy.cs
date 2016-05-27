﻿using System;
using System.Linq;
using System.Security.Cryptography;
using Ecng.Collections;
using Ecng.Common;
using NLog;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using Strategies.Common;
using Strategies.Settings;


namespace Strategies.Strategies
{
    public class ChStrategy : Strategy, IOptionalSettings

    {
        private static readonly Logger TradesLogger = NLog.LogManager.GetLogger("TradesLogger");

        private ICandleManager _candleManager;
        private bool _sendOrder;
        private CandleSeries _series;
        private bool _isFinish;
        private decimal _buyPriceCh;
        private decimal _sellPriceCh;
        private decimal _midPriceCh;

        private readonly SimpleMovingAverage _indicatorSlowSma = new SimpleMovingAverage();
        private readonly SimpleMovingAverage _indicatorFastSma = new SimpleMovingAverage();
        private readonly Highest _indicatorHighest = new Highest();
        private readonly Lowest _indicatorLowest = new Lowest();

        private static int _cancelCandle = 3;
        private int _cancelOrderCandle = _cancelCandle;

        private decimal _highestValue;
        private decimal _lowestValue;
        private decimal _midChValue;
        private decimal _ssmaValue;
        private decimal _fsmaValue;
        private bool _exitPosition;
        private bool _enterPosition;
        public ChStrategy()
        {
            // _timeFrame = this.Param("TimeFrame", TimeSpan.FromMinutes(1));
        }
        public ChStrategy(SerializableDictionary<string, object> settingsStorage)
        {
            object obj;
            //когда меняется выбранный элемент, не меняется набор параметров.
            settingsStorage.TryGetValue(ChStrategyDefaultSettings.TimeFrameString, out obj);

            TimeSpan tstest = new TimeSpan(0, 0, int.Parse(obj.ToString()));
            //TimeSpan ts = TimeSpan.ParseExact(obj.ToString(),"ss", CultureInfo.InvariantCulture);


            _timeFrame = this.Param(ChStrategyDefaultSettings.TimeFrameString, tstest);

            settingsStorage.TryGetValue(ChStrategyDefaultSettings.FastSmaString, out obj);
            var fs = (decimal)obj;
            _fastSma = this.Param(ChStrategyDefaultSettings.FastSmaString, fs);

            settingsStorage.TryGetValue(ChStrategyDefaultSettings.SlowSmaString, out obj);
            var ss = (decimal)obj;
            _slowSma = this.Param(ChStrategyDefaultSettings.SlowSmaString, ss);

            settingsStorage.TryGetValue(ChStrategyDefaultSettings.PeriodString, out obj);
            var per = (decimal)obj;
            _period = this.Param(ChStrategyDefaultSettings.PeriodString, per);



            _indicatorSlowSma.Length = Convert.ToInt32(_slowSma.Value.ToString());


            _indicatorFastSma.Length = Convert.ToInt32(_fastSma.Value.ToString());


            _indicatorHighest.Length = Convert.ToInt32(_period.Value.ToString());



            _indicatorLowest.Length = Convert.ToInt32(_period.Value.ToString());

        }



        private StrategyParam<TimeSpan> _timeFrame;
        private StrategyParam<decimal> _fastSma;
        private StrategyParam<decimal> _slowSma;
        private StrategyParam<decimal> _period;

        public TimeSpan TimeFrame
        {
            get { return _timeFrame.Value; }
            set
            {
                if (value == TimeFrame)
                    return;
                _timeFrame.Value = value;
            }
        }

        public decimal FastSma
        {
            get { return _fastSma.Value; }
            set
            {
                if (value == FastSma)
                    return;
                _fastSma.Value = value;
            }
        }

        public decimal SlowSma
        {
            get { return _slowSma.Value; }
            set
            {
                if (value == SlowSma)
                    return;
                _slowSma.Value = value;
            }
        }

        public decimal Period
        {
            get { return _period.Value; }
            set
            {
                if (value == Period)
                    return;
                _period.Value = value;
            }
        }


        public override string Name => GetFriendlyName();

        protected override void OnStarted()
        {
            var dd = Slippage;
            // Вызываем базовую реализацию метода.
            base.OnStarted();

            TradesLogger.Info("{0}: START", Name);

            // Получаем CandleManager 
            _candleManager = this.GetCandleManager();

            // Подписываемся на сделки
            if (!Connector.RegisteredTrades.Contains(Security))
                Connector.RegisterTrades(Security);

            // создаем серию
            _series = new CandleSeries(typeof(TimeFrameCandle), Security, TimeFrame)
            {
                WorkingTime = ExchangeBoard.Forts.WorkingTime
            };

            // создаем правило на появление завершенной свечи
            _candleManager // объект, к которому применяется правило
                .WhenCandlesFinished(_series) // условие (событие) правила
                .Do(GetValueIndicator) // действия при выполнении условия (событие) правила
                .Until(FinishCandles)
                // модифиатор работы правила. В данном случа правило работает до тех пока, FinishCandles не вернет true
                .Apply(this);


            _candleManager // объект, к которому применяется правило
                .WhenCandlesChanged(_series) // условие (событие) правила
                .Do(ChekLeSeLxSx) // действия при выполнении условия (событие) правила
                .Until(FinishCandles)
                // модифиатор работы правила. В данном случа правило работает до тех пока, FinishCandles не вернет true
                .Apply(this);


            _candleManager.Start(_series);

        }

        protected override void OnStopping()
        {
            // _candleManager.Stop(_series);
            _isFinish = true;
            CancelActiveOrders();

        }

        protected override void OnStopped()
        {


            TradesLogger.Info("{0}: STOP", Name);
            

        }



        // Обработчик события появления новой завершенной свечи

        // 1. Создает правила для заявки
        // 2. Регистрирует заявку
        private void OrderProcess(Order order)
        {

            TradesLogger.Info("{0}: New order, price {1}(MarketPrice), {2}, vol {3}", Name, order.Price ,order.Direction, order.VisibleVolume);
            // Создаем правило на событие отмены заявки
           

            
            var orderCanceledRule = order.WhenCanceled(this.Connector).Do(o =>
            {

                _sendOrder = false;
                TradesLogger.Info("{0}: Order Canceled", Name);
                // TODO
            }).Once();

            // Создаем правило на событие регистрации заявки
            var orderRedisterRule = order.WhenRegistered(this.Connector).Do(o =>
            {
                this.AddInfoLog(o.ToString());
                // Переводим в рабочее состояние
                TradesLogger.Info("{0}: Order {1} Registered", Name, order.TransactionId);
                
                orderCanceledRule.Apply(this);
            });

            // Приводим правило в рабочее состояние
            orderRedisterRule.Once().Apply(this);

            order.WhenRegisterFailed(this.Connector).Do((o, of) =>
            {
                _sendOrder = false;
                TradesLogger.Info("{0}: Order register Failed {1}", Name, order.TransactionId);
                this.AddErrorLog(of.Error);
            });


            // Создаем правило на событие полного исполнения заявки
            var orderMatchedRule = order.WhenMatched(this.Connector);

            orderMatchedRule
                .Do(o =>
                {
                    // "Опускаем" флаг. Теперь в ProcessCandles возобновится отработка логики стратегии
                    _sendOrder = false;
                    
                   
                    // Удаляет все правила, связанные с заявкой (удаление правил по токену)
                    this.Rules.RemoveRulesByToken(orderMatchedRule.Token, orderMatchedRule);
                    
                    TradesLogger.Info("{0}: Order {1} finish ", Name,order.TransactionId );
                    // Удаляет определенное правило
                    // this.Rules.Remove(orderCanceledRule)
                })
                .Once()
                .Apply(this);

            // "Возводим" флаг. Теперь в ProcessCandles будет приостановлена отработка логики стратегии
            _sendOrder = true;

            this.RegisterOrder(order);

            order.WhenNewTrades(Connector).Do(trades =>
            {
                var trade = MyTrades.Last();
                TradesLogger.Info("{0}: Trade , Slippage {1}, price {2}, vol {3}", Name ,trade.Slippage , trade.Trade.Price, trade.Trade.Volume );
            })
            .Apply(this);

        }

        // Возвращает объект заявки с рыночной ценой или лимитной, по заданному направлению
        private Order GetOrder(Sides side, decimal price, Candle candle)
        {
            
            var shrinkPrice = Security.ShrinkPrice(price); // Обрезаем цену лимитную до шага цены иснтрумента
            var bestprice = this.GetMarketPrice(side); // цена входа по рынку

            //if (orderprice == null)
            //    return null;
            return this.CreateOrder(side, bestprice);
        }

        // Критерий продолжения работы правила WhenCandlesFinished
        private bool FinishCandles()
        {
            return _isFinish;
        }

        public string GetFriendlyName()
        {

            //Заметка для Артёма: т.к у нас имя считается idшником в системе(на него много чего завязано), оч важно соблюдать некотрые правила. например пробелы в конце имени. там просто не надо их оставлять
            //т.к при фильтрации все это учитывается.
            //TODO: определить единый формат строки имени агента для всех стратегий на платформе.
            return "ChStrategy {0}_{1}_{2}_{3}".Put(_timeFrame.Value.TotalSeconds, _fastSma.Value, _slowSma.Value, _period.Value);
        }

        private void ChekLeSeLxSx(Candle candle) // метод для проверки входа в позицию, вызывается по любому изменению свечки
        {
            Order order = null;

            if (!_sendOrder &&
                _ssmaValue != 0 &&
                _fsmaValue != 0 &&
                _lowestValue != 0 &&
                _highestValue != 0 &&
                _midChValue != 0)
            {
                if (Position == 0) // Проверяем, есть ли активные позиции, если есть активные , то ждем их закрытия
                {
                    if (_ssmaValue > _fsmaValue && candle.LowPrice <= _lowestValue && _enterPosition)
                    // Вход в короткую позицию
                    {
                        _exitPosition = false; // блокируем вход и выход в одной свече
                        _enterPosition = false; // для предотвращения бесконечных входов внутри одной свечки
                        _sellPriceCh = _lowestValue;
                        order = GetOrder(Sides.Sell, _sellPriceCh, candle);
                        TradesLogger.Info("{0}: SE {1}, SSMA {2} > FSMA {3}, Candle_LP {4} <= Lowest {5}", Name,
                            _sellPriceCh, _ssmaValue, _fsmaValue,
                            candle.LowPrice, _lowestValue);
                    }
                    else if (_ssmaValue < _fsmaValue && candle.HighPrice >= _highestValue && _enterPosition)
                    {
                        _exitPosition = false; // блокируем вход и выход в одной свече
                        _enterPosition = false; // для предотвращения бесконечных входов внутри одной свечки
                        _buyPriceCh = _highestValue;
                        order = GetOrder(Sides.Buy, _buyPriceCh, candle);
                        TradesLogger.Info("{0}: LE {1}, SSMA {2} < FSMA {3}, Candle_HP {4} >= Highest {5} ", Name,
                            _buyPriceCh, _ssmaValue, _fsmaValue,
                            candle.LowPrice, _highestValue);
                    }
                }
                else if (Position < 0) // Для закрытия короткой позиции
                {
                    if (candle.HighPrice >= _midChValue && _exitPosition)
                    {
                        _exitPosition = false;// для предотвращения бесконечных выходов внутри одной свечки
                        _enterPosition = false; // блокируем вход и выход в одной свече
                        _midPriceCh = _midChValue;
                        order = GetOrder(Sides.Buy, _midPriceCh, candle);
                        TradesLogger.Info("{0}: SX {1}, Candle_HP {2} > MidCH {3}", Name, _midPriceCh, candle.HighPrice,
                            _midChValue);
                    }
                }
                else // Для закрытия длинной позиции
                {
                    if (candle.LowPrice <= _midChValue && _exitPosition)
                    {
                        _exitPosition = false; // для предотвращения бесконечных выходов внутри одной свечки
                        _enterPosition = false; // блокируем вход и выход в одной свече
                        _midPriceCh = _midChValue;
                        order = GetOrder(Sides.Sell, _midPriceCh, candle);
                        TradesLogger.Info("{0}: LX {1}, Candle_LP {2} < MidCH {3}", Name, _midPriceCh, candle.LowPrice,
                            _midChValue);
                    }
                }

            }

            if (order != null)
                OrderProcess(order);
        }

        private void GetValueIndicator(Candle candle) // получаем значения индикаторов по факту окончания свечки
        {

            var timeFrame = (TimeSpan)candle.Arg;
            var time = ((TimeSpan)candle.Arg).GetCandleBounds(Connector.CurrentTime).Min - timeFrame;
            var highestValue = _indicatorHighest.Process(candle.HighPrice);
            var lowestValue = _indicatorLowest.Process(candle.LowPrice);
            var ssmaValue = _indicatorSlowSma.Process(candle.ClosePrice);
            var fsmaValue = _indicatorFastSma.Process(candle.ClosePrice);



            if (candle.OpenTime < time ||
                !_indicatorSlowSma.IsFormed ||
                !_indicatorFastSma.IsFormed ||
                !_indicatorHighest.IsFormed ||
                !_indicatorLowest.IsFormed ||
                _sendOrder)
            {
                if (_sendOrder &&
                    _indicatorSlowSma.IsFormed &&
                    _indicatorFastSma.IsFormed &&
                    _indicatorHighest.IsFormed &&
                    _indicatorLowest.IsFormed
                    )
                {
                    if (_cancelOrderCandle == 0) // проверяем когда нужно снять все заявки в случае пропуска правильного входа или выхода
                    {
                        CancelActiveOrders();

                        _cancelOrderCandle = _cancelCandle;
                    }
                    else
                    {
                        TradesLogger.Info("{0}: Wait order finish, {1} candles until canceled", Name, _cancelOrderCandle);
                        --_cancelOrderCandle;
                    }

                }
                else
                    TradesLogger.Info("{0}: Historical candles {1}", Name, candle.OpenTime);
                return;
            }

            var midCh = (_indicatorLowest.GetCurrentValue() + _indicatorHighest.GetCurrentValue()) / 2;
            _highestValue = _indicatorHighest.GetCurrentValue();
            _lowestValue = _indicatorLowest.GetCurrentValue();
            _midChValue = midCh;
            _ssmaValue = _indicatorSlowSma.GetCurrentValue();
            _fsmaValue = _indicatorFastSma.GetCurrentValue();

            _enterPosition = true;
            _exitPosition = true;

            TradesLogger.Info("{0}: Position = {6}, SlowSMA {1}, FastSMA {2}, Highest {3}, Lowest {4}, Mid {5}", Name, _ssmaValue, _fsmaValue, _highestValue, _lowestValue, _midChValue, Position);

        }

    }
}
