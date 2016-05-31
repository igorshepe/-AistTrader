using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using MahApps.Metro.Controls;
using NLog;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Plaza;
using StockSharp.Xaml;
using Strategies.Common;
using Strategies.Strategies;
using LogManager = StockSharp.Logging.LogManager;

namespace AistTrader
{
    public partial class MainWindow
    {
        public readonly PlazaTrader Trader = new PlazaTrader();
        public bool IsAgentManagerSettingsLoaded;
        public bool AllAgentManagerItemsChecked { get; set; }
        Strategy strategy = new Strategy();
        public readonly LogManager _logManager = new LogManager(); // Для логирования внутренних событий стратегии
        //public static CandleStrategy strategy = new CandleStrategy();
        private void AddAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new ManagerAddition();
            form.ShowDialog();
            form = null;
        }
        public void DelAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in AgentManagerListView.SelectedItems.Cast<AgentManager>().ToList())
            {
                try
                {
                    AgentManagerStorage.Remove(item);
                    Logger.Info("Agent manager item \"{0}\" has been deleted", item.Name);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message);
                }
            }
            SaveAgentManagerSettings();
        }
        private void InitiateAgentManagerSettings()
        {
            StreamReader sr = new StreamReader("AgentManagerSettings.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<AgentManager>), new Type[] { typeof(AgentManager) });
                var agents = (List<AgentManager>)xmlSerializer.Deserialize(sr);
                sr.Close();
                if (agents == null) return;

                AgentManagerStorage.Clear();
                foreach (var rs in agents)
                {
                    AgentManagerStorage.Add(rs);
                }
                AgentManagerListView.ItemsSource = AgentManagerStorage;
                AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
                if (AgentManagerCollectionView.GroupDescriptions != null && AgentManagerCollectionView.GroupDescriptions.Count == 0)
                    AgentManagerCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
                IsAgentManagerSettingsLoaded = true;
            }
            catch (Exception e)
            {
                IsAgentSettingsLoaded = false;
                sr.Close();
                Logger.Log(LogLevel.Error, e.Message);
                Logger.Log(LogLevel.Error, e.InnerException.Message);
                if (e.InnerException.Message == "Root element is missing.")
                    IsAgentManagerSettingsLoaded = false;
            }
        }
        private void EditAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = AgentManagerListView.SelectedItems.Cast<AgentManager>().ToList();
            foreach (var addQuikWindow in from agentConfigs in listToEdit
                                          let index = AgentManagerStorage.IndexOf(agentConfigs)
                                          where index != -1
                                          select new ManagerAddition(agentConfigs, index))
            {
                addQuikWindow.Title = "Редактирование конфигурации";
                addQuikWindow.ShowDialog();
                //SaveSettings();
            }
        }
        public void AddNewAgentManager(AgentManager settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentManagerStorage.Count)
                AgentManagerStorage[editIndex] = settings;
            else
                try
                {
                    AgentManagerStorage.Add(settings);
                    Logger.Info("Successfully added agent manager - \"{0}\"", settings.Name);
                }
                catch (Exception)
                {
                    Logger.Info("Error adding agent - {0}", settings.Name);
                }
            SaveAgentManagerSettings();
            UpdateAgentManagerListView();
        }
        public void UpdateAgentManagerListView()
        {
            AgentManagerListView.ItemsSource = AgentManagerStorage;
            AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
            AgentManagerCollectionView.Refresh();
        }
        private void SaveAgentManagerSettings()
        {
            try
            {
                List<AgentManager> obj = AgentManagerStorage.Select(a => a).ToList();
                using (var fStream = new FileStream("AgentManagerSettings.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var xmlSerializer = new XmlSerializer(typeof(List<AgentManager>), new Type[] { typeof(AgentManager) });
                    xmlSerializer.Serialize(fStream, obj);
                    fStream.Close();
                }
                Logger.Info("Successfully saved manager agent Items");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }
        private void AgentManagerListView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsAgentManagerSettingsLoaded & (File.Exists("AgentManagerSettings.xml")))
                InitiateAgentManagerSettings();

            if (AgentManagerListView.Items.Count == 0)
            {
                EditAgentManagerBtn.IsEnabled = false;
                DelAgentManagerBtn.IsEnabled = false;
            }
            else
            {
                EditAgentManagerBtn.IsEnabled = true;
                DelAgentManagerBtn.IsEnabled = true;
            }
        }
        private void TestStrategyStartBtnClick(object sender, RoutedEventArgs e)
        {
            //var item = AgentManagerListView.SelectedItem as AgentManager;
            //var strategyName =item.AgentManagerSettings.AgentOrGroup.Split(null);
            //var connectionName = item.AgentManagerSettings.Account.Connection.Name;
            //Strategy strategy = null;
            //var strategyType = HelperStrategies.GetRegistredStrategiesTest(strategyName.FirstOrDefault());

            //strategy =  (Strategy)Activator.CreateInstance(strategyType);
            //{
            //    strategy.Security = item.AgentManagerSettings.Tool;
            //    strategy.Portfolio = item.AgentManagerSettings.Account.Connection.Connection.Accounts.FirstOrDefault(); //todo: переделать структуру портфеля и добавить короткие оригинальные имена стратегий и подумать как будет запускаться группа
            //    strategy.Connector =ConnectionManager.Connections.FirstOrDefault(i=> i.Name == connectionName);
            //}
            //strategy.Start();

            //var strategytest = new ChStrategy
            //{
            //    Security = ConnectionManager.Connections[0].Securities.First(i => i.Code == "SiH6"),
            //    Connector = ConnectionManager.Connections.First()
            //};
            //strategytest.Start();
        }

        private void AgentManagerListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentManagerListView.Items.Count == 0)
            {
                EditAgentManagerBtn.IsEnabled = false;
                DelAgentManagerBtn.IsEnabled = false;
            }
            else
            {
                EditAgentManagerBtn.IsEnabled = true;
                DelAgentManagerBtn.IsEnabled = true;
            }
        }

        private void AgentManagerTradeSettingsPic_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var item = frameworkElement.DataContext;
            }
            var form = new ManagerAdditionTradeSettings();
            form.ShowDialog();
            form = null;
        }

        private void StartStopBtnClick(object sender, RoutedEventArgs e)
        {

            if ((bool)(sender as ToggleSwitchButton).IsChecked)
            {
                //ON 
                var item = (sender as FrameworkElement).DataContext as AgentManager;
                //CandleStrategy strategy = null;
                var strategyName = item.AgentManagerSettings.AgentOrGroup.Split(null);
                var connectionName =
                    AgentPortfolioStorage.Cast<Portfolio>()
                        .FirstOrDefault(i => i.Name == item.AgentManagerSettings.Portfolio.Name);
                var portfolio = item.AgentManagerSettings.Portfolio;
                var realConnection =
                    ConnectionManager.Connections.Find(i =>
                    {
                        return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName;
                    });
                var strategyType = HelperStrategies.GetRegistredStrategiesTest(strategyName.FirstOrDefault());
                SerializableDictionary<string, object> agentSetting = new SerializableDictionary<string, object>();
                var agentName = item.AgentManagerSettings.AgentOrGroup;
                var agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().Select(i => i).Where(i => i.Name == agentName).ToList();
                var firstOrDefault = agent.FirstOrDefault();
                if (firstOrDefault != null) agentSetting = firstOrDefault.Params.SettingsStorage;

                var amount = new UnitEditor();
                amount.Text = item.Amount;
                amount.Value = amount.Text.ToUnit();

                //todo: дописать конвертацию под проценты и расчёт по формуле
                //strategy = new ChStrategy(agentSetting);

                strategy = new Strategy();
                strategy = (ChStrategy)Activator.CreateInstance(strategyType, agentSetting);
                //strategy = new ChStrategy(agentSetting);
                strategy.DisposeOnStop = true;
                strategy.Security = item.AgentManagerSettings.Tool;
                strategy.Portfolio = realConnection.Portfolios.FirstOrDefault(i => i.Name == item.AgentManagerSettings.Portfolio.Code);
                strategy.Connector = realConnection;
                //strategy.Volume = amount.Value();
                strategy.Volume = amount.Value.To<decimal>();
                var candleManager = new CandleManager(realConnection);
                strategy.SetCandleManager(candleManager);
                strategy.LogLevel = LogLevels.Debug;
                strategy.Start();
                // Логирование внутренних событий стратегии для тестов
                _logManager.Sources.Add(strategy);
                _logManager.Listeners.Add(new FileLogListener("LogStrategy {0}_{1:00}_{2:00}.txt".Put(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)));
                _logManager.Listeners.Add(new GuiLogListener(_monitorWindow));


                //item.AgentManagerSettings.Command = OperationCommand.Disconnect;
                //UpdateAgentManagerListView();


                //SerializableDictionary<string, object> agentSetting = new SerializableDictionary<string, object>();






                //var candleManager = new CandleManager(realConnection);
                //var strat = new ChStrategy(agentSetting);
                //strat.Connector = realConnection;
                //strat.TimeFrame = TimeSpan.FromMinutes(1);

                //strat.Portfolio = realConnection.Portfolios.First();
                //strat.Security = realConnection.Securities.First(i => i.Code == "SiM6");
                //strat.Volume = 1;
                //strat.SetCandleManager(candleManager);
                //strat.LogLevel = LogLevels.Debug;
                //strat.Start();

                //var rowItem = Instance.ConnectionsStorage.FirstOrDefault(i => i == item);
                //int index = ConnectionManager.Connections.FindIndex(i => i.ConnectionName == rowItem.DisplayName);
                //rowItem.ConnectionParams.Command = OperationCommand.Disconnect;
                //if (rowItem.ConnectionParams.IsRegistredConnection)
                //    ConnectionManager.Connections[index].Connect();
                //else
                //    ConnectAccount(item as Connection);
                //UpdateProviderListView();
            }
            else
            {
                var item = (sender as FrameworkElement).DataContext as AgentManager;
                strategy.Stop();


                if (item != null) item.AgentManagerSettings.Command = OperationCommand.Connect;
                UpdateAgentManagerListView();
            }
        }

        private void ConnectTest_OnClick(object sender, RoutedEventArgs e)
        {
            string adress = IPAddress.Loopback.ToString() + ":" + 4001 /*port*/;

            Trader.Address = adress.To<IPEndPoint>();
            Trader.IsCGate = true;
            Trader.IsDemo = true;
            Trader.Connect();
        }

        private void StartStrategyTest_OnClick(object sender, RoutedEventArgs e)
        {
            //var candleManager = new CandleManager(Trader);
            //var strat = new CandleStrategy();
            //strat.Connector = Trader;
            //strat.TimeFrame = TimeSpan.FromMinutes(1);

            //strat.Portfolio = Trader.Portfolios.First();
            //strat.Security = Trader.Securities.First(i => i.Code == "SiM6");
            //strat.Volume = 1;
            //strat.SetCandleManager(candleManager);
            //strat.LogLevel = LogLevels.Debug;

            //strat.Start();
        }


        private void ChkBoxSelectAllAgentManagerItems_OnClick(object sender, RoutedEventArgs e)
        {

            if (AllAgentsChecked == true)
            {

                //foreach (var item in AgentListView.Items.Cast<Agent>())
                //{
                //    item.Params.IsChecked = false;
                //}

                var list = AgentListView.Items.Cast<Agent>().Select(i => i).ToList();
                foreach (var i in list)
                    i.Params.IsChecked = true;
                AgentListView.CommitEdit();
                AgentListView.CommitEdit();
                AgentListView.CancelEdit();
                AgentListView.CancelEdit();
                ICollectionView view = CollectionViewSource.GetDefaultView(AgentListView.Items);
                view.Refresh();
            }
            else
            {
                var list = AgentListView.Items.Cast<Agent>().Select(i => i).ToList();
                foreach (var i in list)
                    i.Params.IsChecked = false;
                AgentListView.CommitEdit();
                AgentListView.CommitEdit();
                AgentListView.CancelEdit();
                AgentListView.CancelEdit();
                ICollectionView view = CollectionViewSource.GetDefaultView(AgentListView.Items);
                view.Refresh();
            }
            // throw new NotImplementedException();
        }
    }
}
