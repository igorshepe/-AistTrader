using System;
using System.Collections.Generic;
using System.Linq;
using Ecng.Common;
using StockSharp.Algo;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using Strategies.Common;

namespace Strategies
{
    public abstract class BaseStrategy : TimeFrameStrategy
    {
        protected static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(15);
        protected SerializableDictionary<string, object> SettingsStorage { get; set; }

        protected BaseStrategy() : base(TimeSpan.FromMinutes(5)) { }

        protected BaseStrategy(SerializableDictionary<string, object> settingsStorage, TimeSpan? timeFrame = null)
            : base(timeFrame ?? TimeSpan.FromMinutes(5))
        {
            CancelOrdersWhenStopping = false;

            SettingsStorage = settingsStorage;

            Interval = TimeSpan.FromSeconds(5);
            Volume = 1;
        }

        public abstract string GetFriendlyName();

        public override string Name
        {
            get
            {
                return GetFriendlyName();
            }
        }

        protected override void OnStarted()
        {
            this.AddInfoLog("Добавляем ордера в стратегию.");
            AddOrders(Connector.Orders);

            base.OnStarted();
        }

        private void AddOrders(IEnumerable<Order> orders)
        {
            //foreach (var order in orders.Where(o => o.Portfolio == Portfolio && o.Security == Security).ToList())
            //{
            //    this.AddInfoLog("Добавляем заявку {0} в стратегию.".Put(order.Id));
            //    AddOrders(order);
            //}
        }

        protected decimal GetCurrentPosition()
        {
            var pos = Connector.GetPosition(Portfolio, Security);
            return pos != null ? pos.CurrentValue : 0;
        }

        protected override void OnError(Exception error)
        {
            CancelAllOrders();
            base.OnError(error);
        }

        protected void CancelAllOrders()
        {
            if (ProcessState == ProcessStates.Stopping) return;

            this.AddInfoLog("Снимаем все заявки");
            Connector.CancelOrders(null, Portfolio, null, null, Security);
        }

        protected void ExitAtMarket()
        {
            var currentPosition = GetCurrentPosition();

            if (currentPosition == 0)
                return;

            Volume = Math.Abs(currentPosition);

            if (currentPosition > 0)
                SellStrategyVolumeAtMarket();
            else
                BuyStrategyVolumeAtMarket();
        }

        protected void BuyStrategyVolumeAtMarket()
        {
            if (Volume == 0)
                return;

            this.AddInfoLog("Регистрируем заявку на покупку по рынку.");

            //var newOrder = this.BuyAtLimit(Security.MaxPrice);
           // RegisterOrder(newOrder);
        }

        protected void SellStrategyVolumeAtMarket()
        {
            if (Volume == 0)
                return;

            this.AddInfoLog("Регистрируем заявку на продажу по рынку.");

            //var newOrder = this.SellAtLimit(Security.MinPrice);
            //RegisterOrder(newOrder);
        }

        protected void PlaceStrategyStop(Sides direction, decimal stopPrice, decimal? volume = null, DateTime? expiryDate = null)
        {
            this.AddInfoLog("Регистрируем стоп-заявку на {0} по цене {1} с объёмом {2}.",
                            direction == Sides.Sell ? "продажу" : "покупку", stopPrice, volume ?? Volume);
            if (expiryDate.HasValue)
                this.AddInfoLog("Срок истечения стоп-заявки: {0}.", expiryDate.Value.ToString("dd.MM.yyyy"));

            var price = direction == Sides.Buy ? stopPrice * 1.0075m : stopPrice * 0.9925m;
            price = Security.ShrinkPrice(price);

            var newStopOrder = new Order
            {
                Portfolio = Portfolio,
                Type = OrderTypes.Conditional,
                Volume = volume ?? Volume,
                Security = Security,
                Direction = direction,
                Price = price,
                ExpiryDate = expiryDate ?? DateTime.MaxValue,
                //Condition = new QuikOrderCondition
                //{
                //    Type = QuikOrderConditionTypes.StopLimit,
                //    StopPrice = stopPrice
                //}
            };
            RegisterOrder(newStopOrder);
        }

        protected bool IfWaitPositionChanged(decimal currentPosition)
        {
            //Ожидание и доп. проверка, т.к. могли не успеть придти сделки по стоп-заявке
            TimeSpan.FromSeconds(3).Sleep();

            if (currentPosition != Position)
            {
                this.AddInfoLog("Успела измениться текущая позиция. Была {0}, стала {1}.".Put(currentPosition, Position));
                return true;
            }

            return false;
        }

        public override void RegisterOrder(Order order)
        {
            order.Comment = Name;

            base.RegisterOrder(order);
        }

        protected bool CheckStop(decimal currentPosition)
        {
            Order order;
            return CheckStop(currentPosition, out order);
        }

        protected bool CheckStop(decimal currentPosition, out Order stop)
        {
            stop = null;

            var lastStopOrder =
                StopOrders.Where(
                    order => order.State == OrderStates.Active && order.Portfolio == Portfolio && order.Security == Security).ToList();

            if (lastStopOrder.Count == 1)
            {
                var direction = currentPosition > 0 ? Sides.Sell : Sides.Buy;
                stop = lastStopOrder[0];
                var result = stop.Direction == direction && stop.Volume == Math.Abs(currentPosition);
                if (!result)
                {
                    stop = null;
                }

                return result;
            }
            else if (lastStopOrder.Count > 1)
            {
                this.AddInfoLog("Слишком много активных стоп-заявок для текущей позиции {0}. Снимаем все.", currentPosition);
                CancelAllOrders();
                return false;
            }
            else if (lastStopOrder.Count == 0)
            {
                if (IfWaitPositionChanged(currentPosition))
                {
                    return true;
                }

                this.AddInfoLog("Не найдена активная стоп-заявка для текущей позиции {0}.", currentPosition);
                return false;
            }

            this.AddInfoLog("Число активных стоп-заявок < 0.");
            return false; // < 0 :)
        }

        protected override ProcessResults OnProcess()
        {
            return ProcessResults.Continue;
        }
    }
}
