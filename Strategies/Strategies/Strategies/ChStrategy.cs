using System;
using System.Linq;
using System.Threading.Tasks;
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
        private Order _registeredOrder;
        private readonly string _nameGroup;
        private string _nameStrategy;
        private bool _firstLap = true;
        public ChStrategy()
        {

        }

        public ChStrategy(SerializableDictionary<string, object> settingsStorage, string nameGroup)
        {
            _nameGroup = nameGroup;
            object obj;
            //когда меняется выбранный элемент, не меняется набор параметров.
            settingsStorage.TryGetValue(ChStrategyDefaultSettings.TimeFrameString, out obj);

            TimeSpan tstest = new TimeSpan(0, 0, int.Parse(obj.ToString()));


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


        private string CheckNameGroup()
        // проверка состоит ли стратегия в группе, если да, то добавить название группы к имени страты
        {
            string nameStrategy;

            if (_nameGroup != "single")
            {
                nameStrategy = $"[{_nameGroup}] {GetFriendlyName()}";
            }
            else
            {
                nameStrategy = GetFriendlyName();
            }


            return nameStrategy;
        }

        protected override void OnStarted()
        {
            _nameStrategy = CheckNameGroup();





            base.OnStarted();

            //TODO: Артём, привет)
            //задание тебе небольшое по части оптимизации
            // в этих методах CheckNameGroup,_nameStrategy,GetFriendlyName поставь брекпоинты и запусти на исполнение стратеги из менеджера агентов, ну или группу агентов.
            //попробуй придумать альтернативную версию

            // и еще обрати внимание на имя экземляра класса стратеги- на выходе оно получается искаженное- то есть не возвращает настоящее имя стратеги, а еще и добавляет перед именем имя группы.
            //надо сделать так, что бы мне в платформу при создании экземпляра класса статегии возвращалось ее не искаженное имя.



            Task.Run(() => TradesLogger.Info("{0}: START, Security: {1}", _nameStrategy, Security.Code));

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


            Task.Run(() => TradesLogger.Info("{0}: STOP", _nameStrategy));


        }



        // Обработчик события появления новой завершенной свечи

        // 1. Создает правила для заявки
        // 2. Регистрирует заявку
        private void OrderProcess(Order order)
        {
            try
            {
                Task.Run(() => TradesLogger.Info("{0}: New order, price {1}, {2}, vol {3}, Security: {4}", _nameStrategy, order.Price, order.Direction, order.VisibleVolume, Security.Code));
                // Создаем правило на событие отмены заявки



                var orderCanceledRule = order.WhenCanceled(Connector).Do(o =>
                {

                    _sendOrder = false;
                    Task.Run(() => TradesLogger.Info("{0}: Order Canceled, error {1}, Security: {2}", _nameStrategy, o.Messages, Security.Code));
                    // TODO
                }).Once();




                // Создаем правило на событие регистрации заявки
                var orderRedisterRule = order.WhenRegistered(Connector).Do(o =>
                {
                    _registeredOrder = order;
                    this.AddInfoLog(o.ToString());
                    // Переводим в рабочее состояние
                    Task.Run(() => TradesLogger.Info("{0}: Order {1} Registered, Security: {2}", _nameStrategy, order.TransactionId, Security.Code));

                    orderCanceledRule.Apply(this);
                });

                // Приводим правило в рабочее состояние
                orderRedisterRule.Once().Apply(this);

                order.WhenRegisterFailed(Connector).Do((o, of) =>
                {
                    _sendOrder = false;
                    Task.Run(() => TradesLogger.Info("{0}: Order register Failed {1}, {2}, Security: {3}", _nameStrategy, order.TransactionId, of.Error, Security.Code));
                    this.AddErrorLog(of.Error);
                })
                .Apply(this);


                // Создаем правило на событие полного исполнения заявки
                var orderMatchedRule = order.WhenMatched(Connector);

                orderMatchedRule
                    .Do(o =>
                    {
                        // "Опускаем" флаг. Теперь в ProcessCandles возобновится отработка логики стратегии
                        _sendOrder = false;

                        _cancelOrderCandle = _cancelCandle;
                        // Удаляет все правила, связанные с заявкой (удаление правил по токену)
                        Rules.RemoveRulesByToken(orderMatchedRule.Token, orderMatchedRule);
                        var averagePrice = order.GetAveragePrice(Connector);
                        Task.Run(() => TradesLogger.Info("{0}: Order {1} finish, vol {2} , averagePrice {3:0}, Security: {4}", _nameStrategy, order.TransactionId, order.Volume, averagePrice, Security.Code));
                        // Удаляет определенное правило
                        // this.Rules.Remove(orderCanceledRule)
                    })
                    .Once()
                    .Apply(this);

                // "Возводим" флаг. Теперь в ProcessCandles будет приостановлена отработка логики стратегии
                _sendOrder = true;

                order.WhenNewTrades(Connector).Do(trades =>
                {
                    //var trade = MyTrades.Last();
                    var trade = trades.Last();

                    Task.Run(() => TradesLogger.Info("{0}: Trade price {1:0}, vol {2}, slip {3:0}, Security: {4}", _nameStrategy, trade.Trade.Price, trade.Trade.Volume, trade.Slippage, Security.Code));
                })
                .Apply(this);

                RegisterOrder(order);
            }
            catch (Exception e)
            {
                Task.Run(() => TradesLogger.Info("{0}: Erorr order {1}, Security: {2}", _nameStrategy, e.Source, Security.Code));
                throw;
            }





        }

        // Возвращает объект заявки с рыночной ценой или лимитной, по заданному направлению
        private Order GetOrder(Sides side, decimal price, bool exit)
        {
            var shrinkPrice = Security.ShrinkPrice(price); // Обрезаем цену лимитную до шага цены иснтрумента
            var bestprice = this.GetMarketPrice(side);
            var priceOrder = bestprice;

            var marketDepth = GetMarketDepth(Security); // Загружаем стакан для проверки, входит ли лимитная цена  в его пределы

            if (shrinkPrice <= marketDepth.Asks.Last().Price && shrinkPrice >= marketDepth.Bids.Last().Price) //проверка на лимитную заявку , сработает ли она на бирже
            {
                priceOrder = shrinkPrice;
                Task.Run(() => TradesLogger.Info("{0}: Limit Price in marketDepth, Min {1:0}, Max {2:0}, LimitPrice {3:0}, Security: {4} ", _nameStrategy, marketDepth.Bids.Last().Price,
                    marketDepth.Asks.Last().Price, shrinkPrice, Security.Code));
            }
            else if (exit)
            {
                Task.Run(() => TradesLogger.Info("{0}: MarketPrice! Exit position. Limit price out of range, Min {1:0}, Max {2:0}, LimitPrice {3:0}, Security: {4}", _nameStrategy, marketDepth.Bids.Last().Price,
                    marketDepth.Asks.Last().Price, shrinkPrice, Security.Code));
            }
            else
            {
                Task.Run(() => TradesLogger.Info("{0}: Cancel ORDER! Limit price out of range, Min {1:0}, Max {2:0}, LimitPrice {3:0}, Security: {4}", _nameStrategy, marketDepth.Bids.Last().Price,
                    marketDepth.Asks.Last().Price, shrinkPrice, Security.Code));
                return null;
            }

            //if (shrinkPrice >= Security.MinPrice && shrinkPrice <= Security.MaxPrice) //проверка на лимитную заявку , сработает ли она на бирже
            //{
            //    priceOrder = shrinkPrice;
            //    Task.Run(() => TradesLogger.Info("{0}: Limit Price within a predetermined range, Min {1:0}, Max {2:0}, LimitPrice {3:0}, Security: {4} ", _nameStrategy, Security.MinPrice,
            //        Security.MaxPrice, shrinkPrice, Security.Code));
            //}
            //else if (exit)
            //{
            //    Task.Run(() => TradesLogger.Info("{0}: MarketPrice! Exit position. Limit price out of range, Min {1:0}, Max {2:0}, LimitPrice {3:0}, Security: {4}", _nameStrategy, Security.MinPrice, Security.MaxPrice, shrinkPrice, Security.Code));
            //}
            //else
            //{
            //    Task.Run(() => TradesLogger.Info("{0}: Cancel ORDER! Limit price out of range, Min {1:0}, Max {2:0}, LimitPrice {3:0}, Security: {4}", _nameStrategy, Security.MinPrice, Security.MaxPrice, shrinkPrice, Security.Code));
            //    return null;
            //}




            // // var orderPrice;

            // if (side == Sides.Sell)
            // {
            //     bool priceInDepth = bidList.Last().Price >= shrinkPrice && shrinkPrice <= bidList.First().Price;

            //     Task.Run(()=>TradesLogger.Info(!priceInDepth ? "{0}: Buy LimitPrice out of range Depth, First: {1}, Last: {2}, Shrink: {3} " : "{0}: Buy LimitPrice in Depth, First:{1}, Last:{2}, Shrink{3}", _nameStrategy, bidList.First().Price, bidList.Last().Price, shrinkPrice);
            // }
            // else
            // {
            //     bool priceInDepth = asksList.Last().Price >= shrinkPrice && shrinkPrice <= asksList.First().Price;

            //     Task.Run(()=>TradesLogger.Info(!priceInDepth ? "{0}: Sell LimitPrice out of range Depth, First: {1}, Last: {2}, Shrink: {3}" : "{0}: Sell LimitPrice in Depth, First:{1}, Last:{2}, Shrink{3}", _nameStrategy, asksList.First().Price, asksList.Last().Price, shrinkPrice);
            // }

            //Task.Run(()=>TradesLogger.Info(exit ? "{0}: Exit {1}, {2}" : "{0}: Enter {1}, {2}",_nameStrategy, asksList[0], bidList[0]);


            //Task.Run(() => TradesLogger.Info("{0}: Test No Enter", _nameStrategy));
            //return null;





            if (exit)
            {
                var volExit = Math.Abs(Position); // Обьем всегда положительный, что не сказать про позимцию
                                                  // var vol = Convert.ToInt32(PositionManager.Positions.First(k => k.Key.Item1.SecurityCode == Security.Code).Value); //Разница только в одном. Если у вас стретегия торгует одновременно несколько контрактов (у меня так). В этом случае мой вариант даст объемы каждого контракта. 
                return this.CreateOrder(side, priceOrder, volExit);
            }


            return this.CreateOrder(side, priceOrder);
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

            try
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
                            order = GetOrder(Sides.Sell, _sellPriceCh, false);
                            Task.Run(() => TradesLogger.Info("{0}: SE {1:0}, SSMA {2:0} > FSMA {3:0}, Candle_LP {4:0} <= Lowest {5:0}", _nameStrategy,
                                _sellPriceCh, _ssmaValue, _fsmaValue,
                                candle.LowPrice, _lowestValue));
                        }
                        else if (_ssmaValue < _fsmaValue && candle.HighPrice >= _highestValue && _enterPosition)
                        {
                            _exitPosition = false; // блокируем вход и выход в одной свече
                            _enterPosition = false; // для предотвращения бесконечных входов внутри одной свечки
                            _buyPriceCh = _highestValue;
                            order = GetOrder(Sides.Buy, _buyPriceCh, false);
                            Task.Run(() => TradesLogger.Info("{0}: LE {1:0}, SSMA {2:0} < FSMA {3:0}, Candle_HP {4:0} >= Highest {5:0} ", _nameStrategy,
                                _buyPriceCh, _ssmaValue, _fsmaValue,
                                candle.LowPrice, _highestValue));
                        }
                    }
                    else if (Position < 0) // Для закрытия короткой позиции
                    {
                        if (candle.HighPrice >= _midChValue && _exitPosition)
                        {
                            _exitPosition = false;// для предотвращения бесконечных выходов внутри одной свечки
                            _enterPosition = false; // блокируем вход и выход в одной свече
                            _midPriceCh = _midChValue;
                            order = GetOrder(Sides.Buy, _midPriceCh, true);
                            Task.Run(() => TradesLogger.Info("{0}: SX {1:0}, Candle_HP {2:0} > MidCH {3:0}, Security: {4} ", _nameStrategy, _midPriceCh, candle.HighPrice,
                                _midChValue, Security.Code));
                        }
                    }
                    else // Для закрытия длинной позиции
                    {
                        if (candle.LowPrice <= _midChValue && _exitPosition)
                        {
                            _exitPosition = false; // для предотвращения бесконечных выходов внутри одной свечки
                            _enterPosition = false; // блокируем вход и выход в одной свече
                            _midPriceCh = _midChValue;
                            order = GetOrder(Sides.Sell, _midPriceCh, true);
                            Task.Run(() => TradesLogger.Info("{0}: LX {1:0}, Candle_LP {2:0} < MidCH {3:0}, Security: {4}", _nameStrategy, _midPriceCh, candle.LowPrice,
                                _midChValue, Security.Code));
                        }
                    }

                }

                if (order != null)
                    OrderProcess(order);
            }
            catch (Exception e)
            {
                Task.Run(() => TradesLogger.Info("{0}: Erorr check enter or exit position {1}", _nameStrategy, e.Source));
                throw;
            }

        }

        private void GetValueIndicator(Candle candle) // получаем значения индикаторов по факту окончания свечки
        {

            try
            {
                var timeFrame = (TimeSpan)candle.Arg;
                var time = ((TimeSpan)candle.Arg).GetCandleBounds(Connector.CurrentTime).Min - timeFrame;
                _indicatorHighest.Process(candle.HighPrice);
                _indicatorLowest.Process(candle.LowPrice);
                _indicatorSlowSma.Process(candle.ClosePrice);
                _indicatorFastSma.Process(candle.ClosePrice);



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
                        if (_cancelOrderCandle == 0)
                        // проверяем когда нужно снять все заявки в случае пропуска правильного входа или выхода
                        {
                            CancelActiveOrders();
                            Connector.CancelOrder(_registeredOrder);
                            _sendOrder = false;
                            _cancelOrderCandle = _cancelCandle;
                        }
                        else
                        {
                            Task.Run(
                                () =>
                                    TradesLogger.Info("{0}: Wait order finish, {1} candles until canceled",
                                        _nameStrategy, _cancelOrderCandle));
                            --_cancelOrderCandle;
                        }

                    }
                    else
                    {
                        if (!_firstLap)
                        {
                            Task.Run(() => TradesLogger.Info("{0}: Attention! History , COT {1} < Time {2}, SSMA: {3}, FSMA: {4}, Hi: {5}, Low: {6}, SendOrder: {7} ", _nameStrategy, candle.OpenTime.ToString("H:mm:ss"), time.ToString("H:mm:ss"), _indicatorSlowSma.IsFormed, _indicatorFastSma.IsFormed,
                    _indicatorHighest.IsFormed, _indicatorLowest.IsFormed, _sendOrder));
                        }


                    }

                    return;
                }
                _firstLap = false;
                _midChValue = (_indicatorLowest.GetCurrentValue() + _indicatorHighest.GetCurrentValue()) / 2;
                _highestValue = _indicatorHighest.GetCurrentValue();
                _lowestValue = _indicatorLowest.GetCurrentValue();
                _ssmaValue = _indicatorSlowSma.GetCurrentValue();
                _fsmaValue = _indicatorFastSma.GetCurrentValue();

                _enterPosition = true; // Если есть законченная свечка , можно совершать вход в позицию
                _exitPosition = true; // Если есть законченная свечка , можно совершать выход из позиции

                Task.Run(() => TradesLogger.Info("{0}: Position = {6}, SlowSMA {1:0}, FastSMA {2:0}, Highest {3:0}, Lowest {4:0}, Mid {5:0}", _nameStrategy, _ssmaValue, _fsmaValue, _highestValue, _lowestValue, _midChValue, Position));

            }
            catch (Exception e)
            {
                Task.Run(() => TradesLogger.Info("{0}: Error get indicators value: {1}", _nameStrategy, e.Source));
                throw;
            }

        }

    }
}
