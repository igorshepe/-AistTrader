using System;
using System.Linq;
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
        private bool _isFinish = false;
        private decimal _buyPriceCh;
        private decimal _sellPriceCh;
        private decimal _midPriceCh;
        private bool _exitPosition = false;
        private readonly SimpleMovingAverage _indicatorSlowSma = new SimpleMovingAverage();
        private readonly SimpleMovingAverage _indicatorFastSma = new SimpleMovingAverage();
        private readonly Highest _indicatorHighest = new Highest();
        private readonly Lowest _indicatorLowest = new Lowest();
        private int _cancelOrderCandle = 3;

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
            // Вызываем базовую реализацию метода.
            base.OnStarted();

            TradesLogger.Info("{0}: СТАРТ", Name);

            // Получаем CandleManager 
            _candleManager = this.GetCandleManager();

            // Подписываемся на сделки
            if (!Connector.RegisteredTrades.Contains(Security))
                Connector.RegisterTrades(Security);

            // создаем серию
            _series = new CandleSeries(typeof(TimeFrameCandle), this.Security, TimeFrame)
            {
                WorkingTime = ExchangeBoard.Forts.WorkingTime
            };

            // создаем правило на появление завершенной свечи
            _candleManager // объект, к которому применяется правило
                .WhenCandlesFinished(_series) // условие (событие) правила
                .Do(ProcessCandles) // действия при выполнении условия (событие) правила
                .Until(FinishCandles)
                // модифиатор работы правила. В данном случа правило работает до тех пока, FinishCandles не вернет true
                .Apply(this);
            // Активатор правила (включает правило). В данном случае правило также будет добавлено в список Strategy.Rules


            _candleManager.Start(_series);

        }

        protected override void OnStopping()
        {
            // _candleManager.Stop(_series);
            _isFinish = true;
             
        }

        protected override void OnStopped()
        {


            TradesLogger.Info("{0}: СТОП",Name);
            CancelActiveOrders();

            // Следующая строка добавлена только с демонстрационной целью, т.к.
            // если стратегия добавлена в источники логов LogManager, то сообщения
            // о старте и остановке стратении и так будут сгенерированы.
            //this.AddInfoLog("Стратегия остановлена");
        }

        //private bool NoActiveOrders { get { return Orders.Count(o => o.State == OrderStates.Active) == 0; } }

        // Обработчик события появления новой завершенной свечи
        private void ProcessCandles(Candle candle)
        {
            Order order = null;
            var timeFrame = (TimeSpan)candle.Arg;
            var time = ((TimeSpan)candle.Arg).GetCandleBounds(Connector.CurrentTime).Min - timeFrame;
            var highestValue = _indicatorHighest.Process(candle.HighPrice);
            var lowestValue = _indicatorLowest.Process(candle.LowPrice);
            var ssmaValue = _indicatorSlowSma.Process(candle.ClosePrice);
            var fsmaValue = _indicatorFastSma.Process(candle.ClosePrice);


            if (_cancelOrderCandle == 0)
            {
                CancelActiveOrders();
                _cancelOrderCandle = 5;
                }
            

            if (candle.OpenTime < time ||
                !_indicatorSlowSma.IsFormed ||
                !_indicatorFastSma.IsFormed ||
                !_indicatorHighest.IsFormed ||
                !_indicatorLowest.IsFormed ||
                _sendOrder)
            {
                if (_sendOrder)
                {
                    TradesLogger.Info("{0}: Ожидаем исполнения заявки, {1} свечей до отмены заявки", Name, _cancelOrderCandle);
                    --_cancelOrderCandle;
                }
                else
                    TradesLogger.Info("{0}: Исторические свечи {1}", Name, candle.OpenTime);

                return;
            }

            var midCh = (_indicatorLowest.GetCurrentValue() + _indicatorHighest.GetCurrentValue()) / 2;

            //TradesLogger.Info("Позиций {0}, время открытия свечки {1}", Position, candle.OpenTime);

            if (Position != 0) // Если позиция есть
            {
                TradesLogger.Info("{0}: Позиций = {1}, CP {2}, midCH {3}",Name, Position, candle.ClosePrice, midCh);

                if (Position < 0) // Для короткой позиции
                {
                    if (candle.HighPrice >= midCh)
                    {

                        _exitPosition = true;
                        _midPriceCh = midCh;
                        order = GetOrder(Sides.Buy, _midPriceCh, _exitPosition);
                        TradesLogger.Info("{0}: SX {1}, Candle_HP {2} > MidCH {3}",Name, Sides.Buy, candle.HighPrice, midCh);
                    }
                }
                else // Для длинной позиции
                {

                    if (candle.LowPrice <= midCh)
                    {
                        _exitPosition = true;
                        _midPriceCh = midCh;
                        order = GetOrder(Sides.Sell, _midPriceCh, _exitPosition);
                        TradesLogger.Info("{0}: LX {1}, Candle_LP {2} < MidCH {3}",Name, Sides.Sell, candle.LowPrice, midCh);
                    }


                }

            }
            else if (Position == 0) //Нет активных заявок
            {
                TradesLogger.Info("{0}: Позиций = 0, SlowSMA {1}, FastSMA {2}, Highest {3}, Lowest {4}, Mid {5}",Name, ssmaValue , fsmaValue , highestValue, lowestValue, midCh);

                if (_indicatorSlowSma.GetCurrentValue() > _indicatorFastSma.GetCurrentValue() && candle.LowPrice <= _indicatorLowest.GetCurrentValue())
                {
                    _exitPosition = false;
                    _sellPriceCh = _indicatorLowest.GetCurrentValue();
                    order = GetOrder(Sides.Sell, _sellPriceCh, _exitPosition);
                    TradesLogger.Info("{0}: SE {1}, SSMA {2} > FSMA {3}, Candle_LP {4} <= Lowest {5}",Name, _sellPriceCh, _indicatorSlowSma.GetCurrentValue(), _indicatorFastSma.GetCurrentValue(), candle.LowPrice, _indicatorLowest.GetCurrentValue());

                }
                // 
                else if (_indicatorSlowSma.GetCurrentValue() < _indicatorFastSma.GetCurrentValue() && candle.HighPrice >= _indicatorHighest.GetCurrentValue())
                {
                    _exitPosition = false;
                    _buyPriceCh = _indicatorHighest.GetCurrentValue();
                    order = GetOrder(Sides.Buy, _buyPriceCh, _exitPosition);
                    TradesLogger.Info("{0}: LE {1}, SSMA {2} < FSMA {3}, Candle_HP {4} >= Highest {5} ",Name, _buyPriceCh, _indicatorSlowSma.GetCurrentValue(), _indicatorFastSma.GetCurrentValue(), candle.HighPrice, _indicatorHighest.GetCurrentValue());
                }
            }

            if (order != null)
                OrderProcess(order);

        }

        // 1. Создает правила для заявки
        // 2. Регистрирует заявку
        private void OrderProcess(Order order)
        {
            
            TradesLogger.Info("{0}: Подаем заявку {1}",Name, order.Price);
            // Создаем правило на событие отмены заявки
            
            var orderCanceledRule = order.WhenCanceled(this.Connector).Do(o =>
            {
                
                _sendOrder = false;
                TradesLogger.Info("{0}: Заявка отменена", Name);
                // TODO
            }).Once();

            // Создаем правило на событие регистрации заявки
            var orderRedisterRule = order.WhenRegistered(this.Connector).Do(o =>
            {
                this.AddInfoLog(o.ToString());
                // Переводим в рабочее состояние
                TradesLogger.Info("{0}: Заявка зарегистрирована",Name);
                orderCanceledRule.Apply(this);
            });

            // Приводим правило в рабочее состояние
            orderRedisterRule.Once().Apply(this);

            order.WhenRegisterFailed(this.Connector).Do((o, of) =>
            {
                TradesLogger.Info(of.Error);
                this.AddErrorLog(of.Error);
            });


            // Создаем правило на событие полного исполнения заявки
            var orderMatchedRule = order.WhenMatched(this.Connector);

            orderMatchedRule
                .Do(o =>
                {
                    // "Опускаем" флаг. Теперь в ProcessCandles возобновится отработка логики стратегии
                    _sendOrder = false;
                    
                    TradesLogger.Info("{0}: Заявка исполнена {1}", Name, order.Price);
                    // Удаляет все правила, связанные с заявкой (удаление правил по токену)
                    this.Rules.RemoveRulesByToken(orderMatchedRule.Token, orderMatchedRule);

                    // Удаляет определенное правило
                    // this.Rules.Remove(orderCanceledRule)
                })
                .Once()
                .Apply(this);

            // "Возводим" флаг. Теперь в ProcessCandles будет приостановлена отработка логики стратегии
            _sendOrder = true;

            this.RegisterOrder(order);

        }

        // Возвращает объект заявки с рыночной ценой, по заданному направлению
        private Order GetOrder(Sides side, decimal price, bool exitpos)
        {

            var shrinkPrice = Security.ShrinkPrice(price);

            
            //if (orderprice == null)
            //    return null;
            return this.CreateOrder(side, shrinkPrice);
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

        

    }
}
