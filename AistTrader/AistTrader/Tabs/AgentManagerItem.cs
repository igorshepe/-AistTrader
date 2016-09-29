using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
using Portfolio = Common.Entities.Portfolio;

namespace AistTrader
{
    public partial class MainWindow
    {
        public bool AllManagerAgentsChecked
        {
            set
            {
                _allManagerAgentsChecked = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AllManagerAgentsChecked"));
            }
        }

        public bool _allManagerAgentsChecked;
        public AistTraderStrategiesConnnectionManager AgentConnnectionManager;
        public readonly PlazaTrader Trader = new PlazaTrader();
        public bool IsAgentManagerSettingsLoaded;
        public bool AllAgentManagerItemsChecked { get; set; }
        Strategy strategy = new Strategy();
        public readonly LogManager _logManager = new LogManager(); // Для логирования внутренних событий стратегии

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
                MessageBoxResult resultMsg = MessageBox.Show("Selected agent/group will be permanently deleted! Confirm?", "Delete agent/group", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                if (resultMsg == MessageBoxResult.Yes)
                {
                    try
                    {
                        AgentManagerStorage.Remove(item);
                        Task.Run(() => Logger.Info("Agent manager item \"{0}\"/\"{1}\" has been deleted", item.Name, item.Alias));
                        SaveAgentManagerSettings();
                    }
                    catch (Exception ex)
                    {
                        Task.Run(() => Logger.Log(LogLevel.Error, ex.Message));
                    }
                }
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
                addQuikWindow.Title = "Aist Trader - Edit Configuration";
                addQuikWindow.ShowDialog();
            }
        }

        public void AddNewAgentManager(AgentManager settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentManagerStorage.Count)
            {
                AgentManagerStorage[editIndex] = settings;
            }
            else
            {
                try
                {
                    AgentManagerStorage.Add(settings);
                    Task.Run(() => Logger.Info("Successfully added agent manager - \"{0}\"", settings.Name));
                }
                catch (Exception)
                {
                    Task.Run(() => Logger.Info("Error adding agent - {0}", settings.Name));
                }
            }
            SaveAgentManagerSettings();
            UpdateAgentManagerListView();
        }

        public void UpdateAgentManagerListView()
        {
            AgentManagerListView.ItemsSource = AgentManagerStorage;
            AgentManagerCollectionView =(CollectionView) CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
            AgentManagerCollectionView.Refresh();
        }

        private void SaveAgentManagerSettings()
        {
            try
            {
                List<AgentManager> obj = AgentManagerStorage.Select(a => a).ToList();
                var tList = new List<Type>();
                tList.Add(typeof(Common.Entities.Portfolio));
                tList.Add(typeof(System.TimeZoneInfo));
                tList.Add(typeof(TimeZoneInfo.AdjustmentRule[]));
                tList.Add(typeof(TimeZoneInfo.AdjustmentRule));
                tList.Add(typeof(TimeZoneInfo.TransitionTime));
                tList.Add(typeof(System.DayOfWeek));
                using (var fStream = new FileStream("AgentManagerSettings.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(List<AgentManager>), tList);
                    xmlSerializer.WriteObject(fStream, obj);
                    fStream.Close();
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => Logger.Log(LogLevel.Error, ex.Message));
            }
        }

        private void InitiateAgentManagerSettings()
        {
            using (FileStream fs = new FileStream("AgentManagerSettings.xml", FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var tList = new List<Type>();
                    tList.Add(typeof(Common.Entities.Portfolio));
                    tList.Add(typeof(System.TimeZoneInfo));
                    tList.Add(typeof(TimeZoneInfo.AdjustmentRule[]));
                    tList.Add(typeof(TimeZoneInfo.AdjustmentRule));
                    tList.Add(typeof(TimeZoneInfo.TransitionTime));
                    tList.Add(typeof(System.DayOfWeek));
                    var xmlSerializer = new DataContractSerializer(typeof(List<AgentManager>), tList);
                    var agents = (List<AgentManager>)xmlSerializer.ReadObject(fs);
                    fs.Close();
                    if (agents == null) { return; }

                    AgentManagerStorage.Clear();
                    foreach (var rs in agents)
                    {
                        AgentManagerStorage.Add(rs);
                    }
                    AgentManagerListView.ItemsSource = AgentManagerStorage;
                    AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
                    if (AgentManagerCollectionView.GroupDescriptions != null &&
                        AgentManagerCollectionView.GroupDescriptions.Count == 0)
                    {
                        AgentManagerCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
                    }
                    IsAgentManagerSettingsLoaded = true;
                    SetConnectionCommandStatus();
                }
                catch (Exception e)
                {
                    IsAgentSettingsLoaded = false;
                    fs.Close();
                    Task.Run(() => Logger.Log(LogLevel.Error, e.Message));
                    Task.Run(() => Logger.Log(LogLevel.Error, e.InnerException.Message));
                    if (e.InnerException.Message == "Root element is missing.")
                    {
                        IsAgentManagerSettingsLoaded = false;
                    }
                }
            }
        }

        private void AgentManagerListView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsAgentManagerSettingsLoaded & (File.Exists("AgentManagerSettings.xml")))
            {
                InitiateAgentManagerSettings();
            }
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

        private void StartStopBtnClickTest(object sender, RoutedEventArgs e)
        {
            if ((bool) (sender as ToggleSwitchButton).IsChecked)
            {
                //ON 
                var agentOrGroup = (sender as FrameworkElement).DataContext as AgentManager;

                if (agentOrGroup.AgentManagerSettings.IsConnected) { return; }
                var isActiveConnection = ActiveConnectionCheck(agentOrGroup);

                //начало логики активации контрола из менеджера агентов
                if (!isActiveConnection)
                {
                    MessageBox.Show("Related connections is not active, can't start with no active connection.");
                    
                    var item = AgentManagerCollectionView.Cast<AgentManager>().FirstOrDefault(i => i.Alias == agentOrGroup.Alias);
                    item.AgentManagerSettings.IsConnected = false;
                    UpdateAgentManagerListView();
                    return;
                }
                agentOrGroup.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Stop;
                agentOrGroup.AgentManagerSettings.IsConnected = true;
                StartAgentOrGroup(agentOrGroup);
            }
            else
            {
                //OFF
                var item = (sender as FrameworkElement).DataContext as AgentManager;
                if (!item.AgentManagerSettings.IsConnected) { return; }
                var agentOrGroup = AgentManagerStorage.FirstOrDefault(i => i.Alias == item.Alias.ToString());
                //отдельную логику под остановку групп
                var groupElements = MainWindow.Instance.AgentsStorage.Select(i => i).Where(i => i.Params.GroupName == agentOrGroup.AgentManagerSettings.AgentOrGroup).ToList();
                if (groupElements.Count >= 2)
                {
                    foreach (var agents in groupElements)
                    {
                        var agentsToStop = AgentConnnectionManager.Strategies.Where(i => i.AgentOrGroupName == agentOrGroup.ToString()).ToList();
                        foreach (var agent in agentsToStop)
                        {
                            agent.ActualStrategyRunning.Stop();
                        }
                        if (item != null)
                        {
                            item.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Start;
                        }
                        item.AgentManagerSettings.IsConnected = false;
                    }
                }
                else
                {
                    var strategyOrGroup = AgentConnnectionManager.Strategies.FirstOrDefault(i => i.AgentOrGroupName == item.Alias) as AistTraderAgentManagerWrapper;
                    strategyOrGroup.ActualStrategyRunning.Stop();
                    if (item != null)
                    {
                        item.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Start;
                    }
                    item.AgentManagerSettings.IsConnected = false;
                }
            }
        }

        private bool ActiveConnectionCheck(AgentManager agentOrGroupName)
        {
            var connectionName = AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == agentOrGroupName.AgentManagerSettings.Portfolio.Name);
            var realConnection = ConnectionManager.Connections.Find(i => { return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName; });
            return !(realConnection.IsNull() || realConnection.ConnectionState != ConnectionStates.Connected);
        }

        private string ActiveConnectionName(AgentManager agentOrGroupName)
        {
            var portfolioName = AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == agentOrGroupName.AgentManagerSettings.Portfolio.Name);
            return portfolioName.Connection.DisplayName;
        }

        public decimal? CalculateAmount(AgentManager am, Agent a)
        {
            var connectionName = AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == am.AgentManagerSettings.Portfolio.Name);
            var realConnection = ConnectionManager.Connections.Find(i => { return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName; });
            var amount = new UnitEditor();
            amount.Text = a.Params.Amount;
            amount.Value = amount.Text.ToUnit();
            decimal? calculatedAmount = 0;
            if (amount.Value.Type == UnitTypes.Percent)
            {
                var data = MainWindow.Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == am.AgentManagerSettings.Portfolio.Connection.Id);
                var secG = realConnection.Securities.FirstOrDefault(i => i.Code == am.Tool);
                var secMargSell = data.Securities.FirstOrDefault(i => i.Code == secG.Code);
                var currValue = data.Portfolios.FirstOrDefault(i => i.Name == am.AgentManagerSettings.Portfolio.Code).CurrentValue;
                var percent = amount.Value.Value;
                var calculatedPercent = (currValue / 100) * percent;
                calculatedAmount = calculatedPercent / secMargSell.MarginSell; //todo вот это значение стало приходить как нулл, уточнить у SS почему
                                                                               //todo - уточнить у Дена по округлению от разряда
                decimal truncutedAmountValue = (decimal)calculatedAmount;
                truncutedAmountValue = Math.Truncate(truncutedAmountValue);
                calculatedAmount = truncutedAmountValue;
            }
            if (amount.Value.Type == UnitTypes.Absolute)
            {
                calculatedAmount = amount.Value.To<decimal>();
            }

            return calculatedAmount;
        }

        public decimal? CalculateAmount(AgentManager am)
        {
            var connectionName = AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == am.AgentManagerSettings.Portfolio.Name);
            var realConnection = ConnectionManager.Connections.Find(i => { return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName; });
            var amount = new UnitEditor();
            amount.Text = am.Amount;
            amount.Value = amount.Text.ToUnit();
            decimal? calculatedAmount = 0;
            if (amount.Value.Type == UnitTypes.Percent)
            {
                var data = Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == am.AgentManagerSettings.Portfolio.Connection.Id);
                var secG = realConnection.Securities.FirstOrDefault(i => i.Code == am.Tool);
                var secMargSell = data.Securities.FirstOrDefault(i => i.Code == secG.Code);
                var currValue = data.Portfolios.FirstOrDefault(i => i.Name == am.AgentManagerSettings.Portfolio.Code).CurrentValue;
                var percent = amount.Value.Value;
                var calculatedPercent = (currValue / 100) * percent;
                calculatedAmount = calculatedPercent / secMargSell.MarginSell; //todo вот это значение стало приходить как нулл, уточнить у SS почему
                decimal truncutedAmountValue = (decimal)calculatedAmount;
                truncutedAmountValue = Math.Truncate(truncutedAmountValue);
                calculatedAmount = truncutedAmountValue;
            }
            if (amount.Value.Type == UnitTypes.Absolute)
            {
                calculatedAmount = amount.Value.To<decimal>();
            }
            return calculatedAmount;
        }

        public void StartAgentOrGroup(AgentManager agentOrGroup)
        {
            //check whether we work with group or not

            var groupElements = Instance.AgentsStorage.Select(i=>i).Where(i=>i.Params.GroupName == agentOrGroup.AgentManagerSettings.AgentOrGroup).ToList();
            if (groupElements.Count >= 2)
            {
                //ugroup logic
                //collect all agents
                foreach (var groupMember in groupElements)
                {
                    var connectionName = AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Name);
                    var realConnection = ConnectionManager.Connections.Find(i =>{return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName;});
                    var strategyType = HelperStrategies.GetRegistredStrategiesTest(groupMember.Name.Split(null).FirstOrDefault());

                    var amount = new UnitEditor();
                    amount.Text = groupMember.Params.Amount;
                    amount.Value = amount.Text.ToUnit();
                    decimal? calculatedAmount = 0;
                    if (amount.Value.Type == UnitTypes.Percent)
                    {
                        var data = Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == agentOrGroup.AgentManagerSettings.Portfolio.Connection.Id);
                        var secG = realConnection.Securities.FirstOrDefault(i => i.Code == agentOrGroup.Tool);
                        var secMargSell = data.Securities.FirstOrDefault(i => i.Name == secG.Name).MarginSell;
                        var currValue = data.Portfolios.FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Code).CurrentValue;
                        var percent = amount.Value.Value;
                        var calculatedPercent = (currValue / 100) * percent;
                        calculatedAmount = calculatedPercent / secG.MarginSell; //todo вот это значение стало приходить как нулл, уточнить у SS почему
                        decimal truncutedAmountValue = (decimal)calculatedAmount;
                        truncutedAmountValue = Math.Truncate(truncutedAmountValue);
                        calculatedAmount = truncutedAmountValue;
                    }
                    if (amount.Value.Type == UnitTypes.Absolute)
                    {
                        calculatedAmount = amount.Value.To<decimal>();
                    }

                    string nameGroup = agentOrGroup.ToString();
                    var history = new List<long> { 0 };
                    strategy = new Strategy();
                    strategy = (Strategy)Activator.CreateInstance(strategyType, groupMember.Params.SettingsStorage, nameGroup, history);
                    strategy.DisposeOnStop = true;

                    //тест
                    var secutityG = realConnection.Securities.FirstOrDefault(i => i.Code == agentOrGroup.AgentManagerSettings.Tool);
                    if (!string.IsNullOrEmpty(groupMember.Params.Security))
                    {
                        secutityG = realConnection.Securities.FirstOrDefault(i => i.Code == groupMember.Params.Security);
                    }
                    strategy.Security = secutityG;
                    strategy.Portfolio =realConnection.Portfolios.FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Code);
                    strategy.Connector = realConnection;
                    strategy.Volume =(decimal) calculatedAmount;
                    var candleManager = new CandleManager(realConnection);
                    strategy.SetCandleManager(candleManager);
                    strategy.LogLevel = LogLevels.Debug;
                    strategy.Start();
                    // Логирование внутренних событий стратегии для тестов
                    _logManager.Listeners.Add(new GuiLogListener(_monitorWindow));
                    var wrapper = new AistTraderAgentManagerWrapper(agentOrGroup.Alias, strategy);
                    AgentConnnectionManager.Add(wrapper);
                }
            }
            else
            {
                //single agent logic
                //TODO: при добавлении второго коннекта, у нас нас свитч выключается
                var strategyName = agentOrGroup.AgentManagerSettings.AgentOrGroup.Split(null);
                var connectionName = AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Name);
                var portfolio = agentOrGroup.AgentManagerSettings.Portfolio;
                var realConnection =
                    ConnectionManager.Connections.Find(i =>
                    {
                        return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName;
                    });
                
                var strategyType = HelperStrategies.GetRegistredStrategiesTest(strategyName.FirstOrDefault());
                SerializableDictionary<string, object> agentSetting = new SerializableDictionary<string, object>();
                var agentName = agentOrGroup.AgentManagerSettings.AgentOrGroup;
                var agent = Instance.AgentsStorage.Cast<Agent>().Select(i => i).Where(i => i.Name == agentName).ToList();
                var firstOrDefault = agent.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    agentSetting = firstOrDefault.Params.SettingsStorage;
                }

                var amount = new UnitEditor();
                amount.Text = agentOrGroup.Amount;
                amount.Value = amount.Text.ToUnit();
                decimal? calculatedAmount = 0;
                if (amount.Value.Type == UnitTypes.Percent)
                {
                    var data = Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == agentOrGroup.AgentManagerSettings.Portfolio.Connection.Id);
                    var secS = realConnection.Securities.FirstOrDefault(i => i.Code == agentOrGroup.Tool);
                    var secMargSell = data.Securities.FirstOrDefault(i => i.Name == secS.Name).MarginSell;
                    var currValue = data.Portfolios.FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Code).CurrentValue;
                    var percent = amount.Value.Value;
                    var calculatedPercent = (currValue / 100) * percent;
                    calculatedAmount = calculatedPercent / secMargSell;
                    //todo - уточнить у Дена по округлению от разряда
                    decimal truncutedAmountValue = (decimal)calculatedAmount;
                    truncutedAmountValue = Math.Truncate(truncutedAmountValue);
                    calculatedAmount = truncutedAmountValue;
                }
                if (amount.Value.Type == UnitTypes.Absolute)
                {
                    calculatedAmount = amount.Value.To<decimal>();
                }
                string nameGroup = agentOrGroup.ToString();
                strategy = new Strategy();

                var history = new List<long> {0};
                if (agentOrGroup.TransactionIdHistory != null && agentOrGroup.TransactionIdHistory.Count >= 1)
                {
                    history = agentOrGroup.TransactionIdHistory;
                }
                
                strategy = (Strategy)Activator.CreateInstance(strategyType, agentSetting, "single", history);
                strategy.DisposeOnStop = true;
                var convertedSecurity = realConnection.Securities.FirstOrDefault(i => i.Code == agentOrGroup.Tool);
                strategy.Security = convertedSecurity;
                strategy.Portfolio = realConnection.Portfolios.FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Code);
                strategy.Connector = realConnection;
                strategy.Volume = (decimal)calculatedAmount;
                var candleManager = new CandleManager(realConnection);
                strategy.SetCandleManager(candleManager);
                strategy.LogLevel = LogLevels.Debug;
                strategy.Start();
                // Логирование внутренних событий стратегии для тестов
                _logManager.Listeners.Add(new GuiLogListener(_monitorWindow));
                var wrapper = new AistTraderAgentManagerWrapper(agentOrGroup.Alias, strategy);
                AgentConnnectionManager.Add(wrapper);
            }
        }

        //todo: переписать метод и стартовать персональные инструменты если они присвоены
        public void StartAfterEdit(Agent agentToStartAfterEdit, AgentManager agentManagerToStartAfterEdit)
        {
            //single agent after edit logic
            //собираем все необходимое из менеджера запущенных стратегий/групп
            var conn = AgentConnnectionManager;
            var connectionName =AgentPortfolioStorage.Cast<Portfolio>().FirstOrDefault(i => i.Name == agentManagerToStartAfterEdit.AgentManagerSettings.Portfolio.Name);
            var portfolio = agentManagerToStartAfterEdit.AgentManagerSettings.Portfolio;
            var realConnection =
                ConnectionManager.Connections.Find(i =>
                {
                    return connectionName != null && i.ConnectionName == connectionName.Connection.DisplayName;
                });
            var strategyType = HelperStrategies.GetRegistredStrategiesTest(agentToStartAfterEdit.Params.AgentName);
            SerializableDictionary<string, object> agentSetting = new SerializableDictionary<string, object>();
            agentSetting = agentToStartAfterEdit.Params.SettingsStorage;
            var amount = new UnitEditor();
            amount.Text = agentToStartAfterEdit.Params.Amount;
            amount.Value = amount.Text.ToUnit();
            decimal? calculatedAmount = 0;
            if (amount.Value.Type == UnitTypes.Percent)
            {
                var data = MainWindow.Instance.ConnectionManager.Connections.FirstOrDefault(
                        i => i.ConnectionName == agentManagerToStartAfterEdit.AgentManagerSettings.Portfolio.Connection.Id);
                var secS = realConnection.Securities.FirstOrDefault(i => i.Code == agentManagerToStartAfterEdit.Tool);
                var secMargSell = data.Securities.FirstOrDefault(i => i.Name == secS.Name).MarginSell;
                var currValue = data.Portfolios.FirstOrDefault(i => i.Name == agentManagerToStartAfterEdit.AgentManagerSettings.Portfolio.Code)
                        .CurrentValue;
                var percent = amount.Value.Value;
                var calculatedPercent = (currValue / 100) * percent;
                calculatedAmount = calculatedPercent / secMargSell;
                //todo - уточнить у Дена по округлению от разряда
                decimal truncutedAmountValue = (decimal)calculatedAmount;
                truncutedAmountValue = Math.Truncate(truncutedAmountValue);
                calculatedAmount = truncutedAmountValue;
            }
            if (amount.Value.Type == UnitTypes.Absolute)
            {
                calculatedAmount = amount.Value.To<decimal>();
            }
            string nameGroup = agentManagerToStartAfterEdit.ToString();
            strategy = new Strategy();
            List<long> history = new List<long>() { 0 };
            strategy = (Strategy)Activator.CreateInstance(strategyType, agentSetting, nameGroup, history);
            strategy.DisposeOnStop = true;
            var convertedSecurity = realConnection.Securities.FirstOrDefault(i => i.Code == agentManagerToStartAfterEdit.Tool);
            if (!string.IsNullOrEmpty(agentToStartAfterEdit.Params.Security))
            {
                convertedSecurity = realConnection.Securities.FirstOrDefault(i => i.Code == agentToStartAfterEdit.Params.Security);
            }
            strategy.Security = convertedSecurity;
            strategy.Portfolio = realConnection.Portfolios.FirstOrDefault(i => i.Name == agentManagerToStartAfterEdit.AgentManagerSettings.Portfolio.Code);
            strategy.Connector = realConnection;
            strategy.Volume = (decimal)calculatedAmount;
            var candleManager = new CandleManager(realConnection);
            strategy.SetCandleManager(candleManager);
            strategy.LogLevel = LogLevels.Debug;
            strategy.Start();
            // Логирование внутренних событий стратегии для тестов
            _logManager.Sources.Add(strategy);
            _logManager.Listeners.Add(
                new FileLogListener("LogStrategy {0}_{1:00}_{2:00}.txt".Put(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.Now.Day)));
            _logManager.Listeners.Add(new GuiLogListener(_monitorWindow));
            var wrapper = new AistTraderAgentManagerWrapper(agentManagerToStartAfterEdit.Alias, strategy);
            AgentConnnectionManager.Add(wrapper);
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
        }

        private void ChkBoxSelectAllAgentManagerItems_OnClick(object sender, RoutedEventArgs e)
        {

            if (AllAgentsChecked == true)
            {
                var list = AgentListView.Items.Cast<Agent>().Select(i => i).ToList();
                foreach (var i in list)
                {
                    i.Params.IsChecked = true;
                }
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
                {
                    i.Params.IsChecked = false;
                }
                AgentListView.CommitEdit();
                AgentListView.CommitEdit();
                AgentListView.CancelEdit();
                AgentListView.CancelEdit();
                ICollectionView view = CollectionViewSource.GetDefaultView(AgentListView.Items);
                view.Refresh();
            }
        }

        private void StartStopBtnClick(object sender, RoutedEventArgs e)
        {
            var pressedButton = sender as Button;
            if (pressedButton.Content.ToString() == ManagerParams.AgentManagerOperationCommand.Start.ToString())
            {
                //ON 
                var agentOrGroup = (sender as FrameworkElement).DataContext as AgentManager;
                var conName = ActiveConnectionName(agentOrGroup);
                if (conName != null)
                {
                    Task.Run(() => Logger.Info("Checking related connection - \"{0}\"..", conName));
                }
                var isActiveConnection = ActiveConnectionCheck(agentOrGroup);
                if (!isActiveConnection)
                {
                    Task.Run(() => Logger.Error("Related connections - \"{0}\" is not active, can't start with no active connection..", conName));
                    MessageBox.Show("Related connections is not active, can't start with no active connection.");
                    var item = AgentManagerCollectionView.Cast<AgentManager>().FirstOrDefault(i => i.Alias == agentOrGroup.Alias);
                    UpdateAgentManagerListView();
                    return;
                }
                else
                {
                    Task.Run(() => Logger.Info("Starting \"{0}\"..", agentOrGroup.Alias));
                    agentOrGroup.AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Starting;
                    UpdateAgentManagerListView();
                    agentOrGroup.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Stop;
                    StartAgentOrGroup(agentOrGroup);
                    agentOrGroup.AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Running;
                    UpdateAgentManagerListView();
                }
                UpdateAgentManagerListView();
            }
            else
            {
                //OFF
                var item = (sender as FrameworkElement).DataContext as AgentManager;
                Task.Run(() => Logger.Info("Stopping - \"{0}\"..", item.Alias));
                item.AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Stopping;
                UpdateAgentManagerListView();
                var agentOrGroup = AgentManagerStorage.FirstOrDefault(i => i.Alias == item.Alias.ToString());
                var groupElements = MainWindow.Instance.AgentsStorage.Select(i => i).Where(i => i.Params.GroupName == agentOrGroup.AgentManagerSettings.AgentOrGroup).ToList();
                if (groupElements.Count >= 2)
                {
                    foreach (var agents in groupElements)
                    {
                        var agentsToStop = AgentConnnectionManager.Strategies.Where(i => i.AgentOrGroupName == agentOrGroup.ToString()).ToList();
                        
                        foreach (var agent in agentsToStop)
                        {
                            Task.Run(() => Logger.Info("Stopping - \"{0}\"..", agent.ActualStrategyRunning.Name));
                            agent.ActualStrategyRunning.Stop();
                            var test = agent.ActualStrategyRunning as ChStrategy;
                            var agentInColl = MainWindow.Instance.AgentsStorage.FirstOrDefault(i => i.Name== agent.ActualStrategyRunning.Name);
                            AgentConnnectionManager.Strategies.Remove(agent);
                        }

                        item.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Start;
                        item.AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Stopped;
                    }
                }
                else
                {
                    var strategyOrGroup = AgentConnnectionManager.Strategies.FirstOrDefault(i => i.AgentOrGroupName == item.Alias) as AistTraderAgentManagerWrapper;
                    ChStrategy agent =  strategyOrGroup.ActualStrategyRunning as ChStrategy;
                    var themIDs = agent.TransactionIDs;
                    var agentManagerStorage = Instance.AgentManagerStorage.Single(i => i.Alias == agent.Name);

                    if(themIDs.Count > 0)
                    {
                        if (agentManagerStorage.TransactionIdHistory == null)
                        {
                            agentManagerStorage.TransactionIdHistory  = new List<long>();
                        }
                        agentManagerStorage.TransactionIdHistory.AddRange(themIDs);
                    }
                   
                    strategyOrGroup.ActualStrategyRunning.Stop();
                    AgentConnnectionManager.Strategies.Remove(strategyOrGroup);
                    item.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Start; ;
                    item.AgentManagerSettings.AgentMangerCurrentStatus =ManagerParams.AgentManagerStatus.Stopped; ;
                }
                SaveAgentManagerSettings();
                UpdateAgentManagerListView();
            }
        }

        #region Aist Trader Agent/Group Manager

        public class AistTraderAgentManagerWrapper
        {
            public AistTraderAgentManagerWrapper(string name, Strategy strategy)
            {
                AgentOrGroupName = name;
                ActualStrategyRunning = strategy;
            }

            public override string ToString()
            {
                return AgentOrGroupName;
            }

            public Strategy ActualStrategyRunning { get; set; }
            public string AgentOrGroupName { get; set; }
        }

        public class AistTraderStrategiesConnnectionManager : IList<AistTraderAgentManagerWrapper>, IDisposable
        {
            public List<AistTraderAgentManagerWrapper> Strategies = new List<AistTraderAgentManagerWrapper>();

            public IEnumerator<AistTraderAgentManagerWrapper> GetEnumerator()
            {
                return Strategies.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(AistTraderAgentManagerWrapper item)
            {
                Strategies.Add(item);
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(AistTraderAgentManagerWrapper item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(AistTraderAgentManagerWrapper[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(AistTraderAgentManagerWrapper item)
            {
                return Strategies.Remove(item);
            }

            public int Count { get; }
            public bool IsReadOnly { get; }
            public int IndexOf(AistTraderAgentManagerWrapper item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, AistTraderAgentManagerWrapper item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public AistTraderAgentManagerWrapper this[int index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        private void ChkBoxSelectAllAgentManagerItems_OnChecked(object sender, RoutedEventArgs e)
        {
            AllManagerAgentsChecked = true;

            var list = AgentManagerListView.Items.Cast<AgentManager>().Select(i => i).ToList();
            foreach (var i in list)
            {
                i.AgentManagerSettings.IsChecked = true;
            }
            AgentManagerListView.CommitEdit();
            AgentManagerListView.CommitEdit();
            AgentManagerListView.CancelEdit();
            AgentManagerListView.CancelEdit();
            ICollectionView view = CollectionViewSource.GetDefaultView(AgentListView.Items);
            view.Refresh();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
        }
    }
}
