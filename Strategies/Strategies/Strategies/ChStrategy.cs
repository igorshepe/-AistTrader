using System;
using System.Linq;
using Ecng.Common;
using Ecng.ComponentModel;
using NLog;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;
using Strategies.Common;
using Strategies.Settings;
using LogManager = StockSharp.Logging.LogManager;

namespace Strategies.Strategies
{
    public class ChStrategy : BaseStrategy
    {
        private static readonly Logger TradesLogger = NLog.LogManager.GetLogger("TradesLogger");
        private CandleSeries _candleSeries;
        private CandleManager _candleManager;
        private readonly MedianPrice _medianPrice = new MedianPrice {};
        private BaseStrategy _baseStrategy;

        public ChStrategy (){}

        private readonly SimpleMovingAverage _indicatorSlowSma = new SimpleMovingAverage
        {
            Length = 100
        };

        private readonly SimpleMovingAverage _indicatorFastSma = new SimpleMovingAverage
        {
            Length = 20
        };


        private readonly Highest _indicatorHighest = new Highest
        {
            Length = 20
        };

        private readonly Lowest _indicatorLowest = new Lowest
        {
            Length = 20
        };

        

        private bool NoActiveOrders { get { return Orders.Count(o => o.State == OrderStates.Active) == 0; } }
        // private DateTime _lastTimeFrame = DateTime.MinValue;


        private void RunProcessGetCandles(TimeSpan timeFrame)
        {
            _candleManager = new CandleManager(Connector);

            var security = Security;
            
            _candleSeries = new CandleSeries(typeof(TimeFrameCandle), security, timeFrame);
            
            _candleManager.Start(_candleSeries);

         }

        private void MainAlgorithm(Candle candle)
        {
            var timeFrame = (TimeSpan)candle.Arg;
            var time = ((TimeSpan)candle.Arg).GetCandleBounds(Connector.CurrentTime).Min - timeFrame;
            var highestValue = _indicatorHighest.Process(candle.HighPrice);
            var lowestValue = _indicatorLowest.Process(candle.LowPrice);
            var ssmaValue = _indicatorSlowSma.Process(candle.ClosePrice);
            var fsmaValue = _indicatorFastSma.Process(candle.ClosePrice);
            var medianPriceValue = _medianPrice.Process(candle);


            if (candle.OpenTime < time ||
                !_indicatorSlowSma.IsFormed ||
                !_indicatorFastSma.IsFormed ||
                !_indicatorHighest.IsFormed ||
                !_indicatorLowest.IsFormed ||
                !_medianPrice.IsFormed)
            {
                TradesLogger.Info("Исторические свечи");
                return;
            }

            if (Position != 0) // Если позиция есть
            {
                TradesLogger.Info("есть позиция {0}", Position);
                if (Position > 0) // Для длинной позиции
                {
                    if (candle.ClosePrice < _medianPrice.GetCurrentValue())
                    {
                        //RegisterOrder(this.SellAtMarket());
                        MakeMarketOrder(Sides.Sell);
                    }   
                }
                else // Для короткой позиции
                {

                    if (candle.ClosePrice > _medianPrice.GetCurrentValue())
                    {
                        //RegisterOrder(this.BuyAtMarket());
                        MakeMarketOrder(Sides.Buy);
                    }


                }
                
            }
            else if (NoActiveOrders)    //Нет активных заявок
            {
                TradesLogger.Info("Позиций  нет, проверка условий входа в позицию. Значения Slow SMA {0},Fast SMA {1}, Highest {2}, Lowest {3}", ssmaValue , fsmaValue , highestValue , lowestValue );
                //TradesLogger.Info("Сравниваем значения с GetCurrentValue Значения Slow SMA {0},Fast SMA {1}, Highest {2}, Lowest {3}", _indicatorSlowSma.GetCurrentValue(), _indicatorFastSma.GetCurrentValue(), _indicatorHighest.GetCurrentValue(), _indicatorLowest.GetCurrentValue());
                //Если значение Roc меньше нуля//при пересечении цены закрытия свечи и скользящей
                if (_indicatorSlowSma.GetCurrentValue() > _indicatorFastSma.GetCurrentValue() && candle.LowPrice <= _indicatorLowest.GetCurrentValue())
                {
                    //RegisterOrder(this.SellAtLimit(_indicatorLowest.GetCurrentValue(), Volume));
                    SellStrategyVolumeAtMarket();
                    _baseStrategy.RegisterOrder(this.SellAtLimit(_indicatorLowest.GetCurrentValue(), Volume));
                    //RegisterOrder(this.SellAtMarket());
                    //MakeMarketOrder(Sides.Sell);
                    //MakeLimitOrder(Sides.Sell, _indicatorLowest.GetCurrentValue());
                    TradesLogger.Info("Короткая позиция. Цена ордера {0}", _indicatorLowest.GetCurrentValue());
                    
                }
                //Пересечение цены закрытия и линии боллинджера вверх
                else if (_indicatorSlowSma.GetCurrentValue() < _indicatorFastSma.GetCurrentValue() && candle.HighPrice >= _indicatorHighest.GetCurrentValue())
                {
                    //RegisterOrder(this.BuyAtLimit(_indicatorHighest.GetCurrentValue(), Volume));
                   BuyStrategyVolumeAtMarket();
                    _baseStrategy.RegisterOrder(this.BuyAtLimit(_indicatorHighest.GetCurrentValue(), Volume)); 
                    //RegisterOrder(this.BuyAtMarket());
                    //MakeMarketOrder(Sides.Buy);
                    //MakeLimitOrder(Sides.Buy, _indicatorHighest.GetCurrentValue());
                    TradesLogger.Info("Длинная позиция . Цена ордера {0}", _indicatorHighest.GetCurrentValue());
                }
            }


        }

        protected override void OnStarted()
        {
            
            var timeFrame = TimeSpan.FromMinutes(5);
            RunProcessGetCandles(timeFrame);
            //Подписываемся на правило события окончания чвечей
            _candleSeries.WhenCandlesFinished()
                   .Do(MainAlgorithm)
                   .Apply();

            //Вызываем базовый метод
            base.OnStarted();

            TradesLogger.Info("start strategy {0}, {1}, {2}", timeFrame, GetFriendlyName(),Security  );
        }

        private void MakeMarketOrder(Sides direction)
        {
            var price = direction == Sides.Buy ? Connector.GetMarketDepth(Security).BestAsk.Price : Connector.GetMarketDepth(Security).BestBid.Price;
            var order = this.CreateOrder(direction, price, Volume);
            RegisterOrder(order);
            TradesLogger.Info(order);
        }

        private void MakeLimitOrder(Sides direction, decimal price)
        {
            var order = this.CreateOrder(direction, price, Volume);
            RegisterOrder(order);
            TradesLogger.Info(order);

        }
         
        public override string GetFriendlyName()
        {
            return "ChStrategy {0} - {1} - {2} - {3}".Put(_indicatorFastSma, _indicatorSlowSma, _indicatorHighest, _indicatorLowest);
        }

         
        
        }
    }

