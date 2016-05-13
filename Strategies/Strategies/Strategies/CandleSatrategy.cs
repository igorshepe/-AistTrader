using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecng.Common;
using Ecng.Serialization;
using Strategies.Common;
using Strategies.Settings;

namespace Strategies.Strategies
{
    using System;
    using System.Linq;
    using System.ComponentModel; 
    using NLog;
    //using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

    using StockSharp.Logging;
    using StockSharp.Algo;
    using StockSharp.Algo.Strategies;
    using StockSharp.Algo.Candles;
    using StockSharp.BusinessEntities;
    using StockSharp.Messages;

    /// <summary>
    /// Пример учебной стратегии к курсу "Стратегии StockSharp"
    /// Стратегия использует завершенные свечи. 
    /// Открывает позицию по рынку:
    /// Long - если текущая свеча белая и Close текущей свечи больше High предыдущей свечи;  
    /// Short - если текущая свеча черная и Close текущей свечи меньше Low предыдущей свечи.
    /// Закрывает позицию по рынку оффсетной заявкой:
    /// Long - если текущая свеча черная;  
    /// Short - если текущая свеча белая;
    /// </summary>
    public class CandleStrategy : Strategy, IOptionalSettings
    {
        private static readonly Logger TradesLogger = NLog.LogManager.GetLogger("TradesLogger");
        private ICandleManager _candleManager;
        private bool _sendOrder;
        private CandleSeries _series;

        private bool _IsFinish = false;

        public CandleStrategy()
        {
            _timeFrame = this.Param("TimeFrame", TimeSpan.FromMinutes(1));
        }
        public CandleStrategy(SerializableDictionary<string, object> settingsStorage)
        {
            
        }

        private readonly StrategyParam<TimeSpan> _timeFrame;
        ///// <summary>
        ///// Тайм-фрейм.
        ///// </summary>
        //[Category("Параметры серии данных")]
        //[PropertyOrder(0)]
        //[DisplayName("Тайм-фрейм")]
        //[Description("Тайм-фрейм серии данных")]
         
        //public CandleStrategy() { }


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

        public override string Name => GetFriendlyName();


        protected override void OnStarted()
        {
            //_candleManager = new CandleManager(Connector);
            // Вызываем базовую реализацию метода.
            base.OnStarted();
            TradesLogger.Info("OnStarted - {0}");
            Console.WriteLine("СТАРТ");

            //Получаем CandleManager
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
            _candleManager                      // объект, к которому применяется правило
                .WhenCandlesFinished(_series)   // условие (событие) правила
                .Do(ProcessCandles)             // действия при выполнении условия (событие) правила
                .Until(FinishCandles)           // модифиатор работы правила. В данном случа правило работает до тех пока, FinishCandles не вернет true
                .Apply(this);                   // Активатор правила (включает правило). В данном случае правило также будет добавлено в список Strategy.Rules

 

            // запускаем CandleManager
            _candleManager.Start(_series);

        }

        protected override void OnStopping()
        {
            // _candleManager.Stop(_series);
            _IsFinish = true;
           
            //Console.WriteLine("Stopping");
        }

        protected override void OnStopped()
        {

            Console.WriteLine("СТОП");

            // Следующая строка добавлена только с демонстрационной целью, т.к.
            // если стратегия добавлена в источники логов LogManager, то сообщения
            // о старте и остановке стратении и так будут сгенерированы.
            TradesLogger.Info("Остановка стратегии");
            this.AddInfoLog("Стратегия остановлена");
        }

        // Обработчик события появления новой завершенной свечи
        private void ProcessCandles(Candle candle)
        {
            var timeFrame = (TimeSpan)candle.Arg;
            var time = ((TimeSpan)candle.Arg).GetCandleBounds(Connector.CurrentTime).Min - timeFrame;

            if (candle.OpenTime < time)
            {

                TradesLogger.Info("Историческая свеча - {0}", candle.OpenTime); 
                return;
            }

            //this.Stop();

            // Не обрабатываем, если:
            // 1. Послана, но еще не исполнена заявка;
            // 2. В серии меньше двух свечей;
            // 3. Не возможно определить "цвет" текущей свечи. 
            if (_sendOrder || _candleManager.GetCandleCount(_series) < 2 || candle.IsWhiteOrBlack() == null)
                return;

            TradesLogger.Info("цикл алгоритма");


            Order order = null;

            bool isWhite = candle.IsWhiteOrBlack().Value;

            TradesLogger.Info("Позиций {0}, {1}", Position, CurrentTime);
            if (this.Position == 0) // пытаемся открыть позицию, если нет открытых позиций
            {
                var prevCandle = _candleManager.GetCandle<TimeFrameCandle>(_series, 1);

                if (isWhite) // если свеча белая
                {
                    TradesLogger.Info("Белая свечка {0}", CurrentTime);
                    if (candle.ClosePrice > prevCandle.HighPrice)
                        order = GetOrder(Sides.Buy);
                }
                else // если свеча черная
                {
                    TradesLogger.Info("Черная свечка {0}", CurrentTime);
                    if (candle.ClosePrice < prevCandle.LowPrice)
                        order = GetOrder(Sides.Sell);
                }
            }
            // пытаемся закрыть позицию, если есть открытые позиции
            else if (this.Position > 0)
            {
                if (!isWhite)
                    order = GetOrder(Sides.Sell);
            }
            else if (this.Position < 0)
            {
                if (isWhite)
                    order = GetOrder(Sides.Buy);
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
            return "Имя стратегии {0} _разделители_ {1}".Put("параметр1", "параметр2");
        }
    }
}
