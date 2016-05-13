using System; 
using System.Diagnostics;
using System.Linq;
using Ecng.Common;
using Ecng.Serialization;
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
         
        private bool _IsFinish = false;
       
       
        public ChStrategy()
        {
           // _timeFrame = this.Param("TimeFrame", TimeSpan.FromMinutes(1));
        }
        public ChStrategy(SerializableDictionary<string, object> settingsStorage)
        {
            object obj;
            settingsStorage.TryGetValue(ChStrategyDefaultSettings.TimeFrameString, out obj);
            TimeSpan ts = TimeSpan.Parse(obj.ToString());
            _timeFrame = this.Param(ChStrategyDefaultSettings.TimeFrameString, ts);

            settingsStorage.TryGetValue(ChStrategyDefaultSettings.FastSmaString, out obj);
            var fs = (decimal) obj ;
            _fastSma = this.Param(ChStrategyDefaultSettings.FastSmaString, fs);

            settingsStorage.TryGetValue(ChStrategyDefaultSettings.SlowSmaString, out obj);
            var ss = (decimal)obj;
            _slowSma = this.Param(ChStrategyDefaultSettings.SlowSmaString, ss);

            settingsStorage.TryGetValue(ChStrategyDefaultSettings.PeriodString, out obj);
            var per = (decimal)obj;
            _period = this.Param(ChStrategyDefaultSettings.PeriodString, per);
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


        private bool NoActiveOrders { get { return Orders.Count(o => o.State == OrderStates.Active) == 0; } }

        private readonly SimpleMovingAverage _indicatorSlowSma = new SimpleMovingAverage
        {
            Length = 20
        };

        private readonly SimpleMovingAverage _indicatorFastSma = new SimpleMovingAverage
        {
            Length = 10
        };


        private readonly Highest _indicatorHighest = new Highest
        {
            Length = 20
        };

        private readonly Lowest _indicatorLowest = new Lowest
        {
            Length = 20
        };

        public override string Name => GetFriendlyName();

        protected override void OnStarted()
        {
            // Вызываем базовую реализацию метода.
            base.OnStarted();

            TradesLogger.Info("СТАРТ");

            // Получаем CandleManager 
            _candleManager = this.GetCandleManager();

            // Подписываемся на сделки
            if (!Connector.RegisteredTrades.Contains(Security))
                Connector.RegisterTrades(Security);

            // создаем серию
            _series = new CandleSeries(typeof (TimeFrameCandle), this.Security, TimeFrame)
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
            _IsFinish = true;
            TradesLogger.Info("Stopping");
        }

        protected override void OnStopped()
        {


            TradesLogger.Info("СТОП");

            // Следующая строка добавлена только с демонстрационной целью, т.к.
            // если стратегия добавлена в источники логов LogManager, то сообщения
            // о старте и остановке стратении и так будут сгенерированы.
            this.AddInfoLog("Стратегия остановлена");
        }



        // Обработчик события появления новой завершенной свечи
        private void ProcessCandles(Candle candle)
        {
            var timeFrame = (TimeSpan) candle.Arg;
            var time = ((TimeSpan) candle.Arg).GetCandleBounds(Connector.CurrentTime).Min - timeFrame;
            var highestValue = _indicatorHighest.Process(candle.HighPrice);
            var lowestValue = _indicatorLowest.Process(candle.LowPrice);
            var ssmaValue = _indicatorSlowSma.Process(candle.ClosePrice);
            var fsmaValue = _indicatorFastSma.Process(candle.ClosePrice);
             

            Order order = null;
            
            if (candle.OpenTime < time ||
                !_indicatorSlowSma.IsFormed ||
                !_indicatorFastSma.IsFormed ||
                !_indicatorHighest.IsFormed ||
                !_indicatorLowest.IsFormed ||
                _sendOrder)
            {
                TradesLogger.Info("Исторические свечи {0}", candle.OpenTime);
                return;
            }

            var midCh = (_indicatorLowest.GetCurrentValue() + _indicatorHighest.GetCurrentValue())/2;

            TradesLogger.Info("Позиций {0}, время открытия свечки {1}", Position, candle.OpenTime);

            if (Position != 0) // Если позиция есть
            {
                TradesLogger.Info("есть позиция {0}, CP {1}, midCH {2}", Position, candle.ClosePrice, midCh);
                 
                if (Position < 0) // Для короткой позиции
                {
                    if (candle.ClosePrice >= midCh)
                    {
                        
                        order = GetOrder(Sides.Buy);
                        TradesLogger.Info("SX {0}, Candle_CP {1} > MidCH {2}", Sides.Buy, candle.ClosePrice, midCh);
                    }
                }
                else // Для длинной позиции
                {

                    if (candle.ClosePrice <= midCh)
                    {
                        
                        order = GetOrder(Sides.Sell);
                        TradesLogger.Info("LX {0}, Candle_CP {1} < MidCH {2}", Sides.Sell, candle.ClosePrice, midCh);
                    }


                }

            }
            else if (NoActiveOrders)    //Нет активных заявок
            {
                TradesLogger.Info("Позиций  нет, проверка условий входа в позицию. Значения Slow SMA {0},Fast SMA {1}, Highest {2}, Lowest {3}, Mid {4}", ssmaValue.ToString(), fsmaValue.ToString(), highestValue, lowestValue, midCh);
                
                if (_indicatorSlowSma.GetCurrentValue() > _indicatorFastSma.GetCurrentValue() && candle.LowPrice <= _indicatorLowest.GetCurrentValue())
                {
                    order = GetOrder(Sides.Sell);
                    TradesLogger.Info("SE {0}, SSMA {1} > FSMA {2}, Candle_LP {3} <= Lowest {4} ", Sides.Sell, _indicatorSlowSma.GetCurrentValue(), _indicatorFastSma.GetCurrentValue(), candle.LowPrice, _indicatorLowest.GetCurrentValue());

                }
                // 
                else if (_indicatorSlowSma.GetCurrentValue() < _indicatorFastSma.GetCurrentValue() && candle.HighPrice >= _indicatorHighest.GetCurrentValue())
                {
                    order = GetOrder(Sides.Buy);
                    TradesLogger.Info("LE {0}, SSMA {1} < FSMA {2}, Candle_HP {3} >= Highest {4} ", Sides.Buy, _indicatorSlowSma.GetCurrentValue(), _indicatorFastSma.GetCurrentValue(), candle.HighPrice, _indicatorHighest.GetCurrentValue());
                }
            }
            
            if (order != null)
                OrderProcess(order);

        }

        // 1. Создает правила для заявки
        // 2. Регистрирует заявку
        private void OrderProcess(Order order)
        {
            TradesLogger.Info("Подаем заявку");
            // Создаем правило на событие отмены заявки
            var orderCanceledRule = order.WhenCanceled(this.Connector).Do(o =>
            {
                // TODO
            }).Once();

            // Создаем правило на событие регистрации заявки
            var orderRedisterRule = order.WhenRegistered(this.Connector).Do(o =>
            {
                this.AddInfoLog(o.ToString());
                // Переводим в рабочее состояние
                orderCanceledRule.Apply(this);
            });

            // Приводим правило в рабочее состояние
            orderRedisterRule.Once().Apply(this);

            order.WhenRegisterFailed(this.Connector).Do((o, of) =>
            {
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
        private Order GetOrder(Sides side)
        {
            var price = this.GetMarketPrice(side);
            if (price == null)
                return null;
            return this.CreateOrder(side, price.Value);
        }

        // Критерий продолжения работы правила WhenCandlesFinished
        private bool FinishCandles()
        {
            return _IsFinish;
        }

        public string GetFriendlyName()
        {
            return "ChStrategy {0} _ {1} _ {2} _ {3} ".Put(_timeFrame.Value, _fastSma.Value, _slowSma.Value, _period.Value );
        }
    }
}
