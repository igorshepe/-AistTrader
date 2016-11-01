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
using Common;
using Ecng.Common;
using MahApps.Metro.Controls;
using NLog;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
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
        private bool[] startStopStartedIndexes;

        public bool AllManagerAgentsChecked
        {
            set
            {
                _allManagerAgentsChecked = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AllManagerAgentsChecked"));
            }
        }
        private List<Strategy> Strategies = new List<Strategy>(); // Создаем коллекцию запущенных стратегий
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
            ResetStarted();
            form = null;
        }

        public void DelAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in AgentManagerListView.SelectedItems.Cast<AgentManager>().ToList())
            {
                bool noDelete = true;
                MessageBoxResult resultMsg = MessageBox.Show("Selected agent/group will be permanently deleted! Confirm?", "Delete agent/group", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                if (resultMsg == MessageBoxResult.Yes)
                {
                    bool isGroup = item.StrategyInGroup != null && item.StrategyInGroup.Count > 0;

                    if (item.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Stopped)
                    {
                        if (isGroup)
                        {
                            foreach (var strategyInGroup in item.StrategyInGroup)
                            {
                                bool doRequest = noDelete = strategyInGroup.Position != 0;
                                var agentToDelete = Instance.AgentManagerStorage.FirstOrDefault(it => it.Name == strategyInGroup.Name);

                                if (doRequest)
                                {
                                    var form = new GroupAdditionDeleteMode(item.Name.ToString());
                                    form.ShowDialog();
                                    var selectedMode = form.SelectedDeleteMode;
                                    var agent = Instance.AgentsStorage.FirstOrDefault(a => a.Name == strategyInGroup.Name);
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                    {
                                        if (agentToDelete != null)
                                        {
                                            agentToDelete.CloseState = StrategyCloseState.NoWait;
                                        }
                                    }
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                    {
                                        if (agentToDelete != null)
                                        {
                                            agentToDelete.CloseState = StrategyCloseState.Wait;
                                        }
                                    }
                                    //doDelete = !form.IsCancelled;
                                }
                                else
                                {
                                    if (agentToDelete != null)
                                    {
                                        agentToDelete.CloseState = StrategyCloseState.None;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool doRequest = noDelete = item.SingleAgentPosition != 0;
<<<<<<< HEAD
                            
=======

>>>>>>> 6f232b3803dfda9f294858acb34b54f33ac68c42
                            var agentToDelete = Instance.AgentManagerStorage.FirstOrDefault(it => it.Alias == item.Alias);
                            if (doRequest)
                            {
                                var form = new GroupAdditionDeleteMode(item.Name.ToString());
                                form.ShowDialog();
                                var selectedMode = form.SelectedDeleteMode;
                                var agent = Instance.AgentsStorage.FirstOrDefault(a => a.Name == item.Alias);
                                if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                {
                                    if (agentToDelete != null)
                                    {
                                        agentToDelete.CloseState = StrategyCloseState.NoWait;
                                    }
                                }
                                if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                {
                                    if (agentToDelete != null)
                                    {
                                        agentToDelete.CloseState = StrategyCloseState.Wait;
                                    }
                                }
                                //doDelete = !form.IsCancelled;
                            }
                            else
                            {
                                if (agentToDelete != null)
                                {
                                    agentToDelete.CloseState = StrategyCloseState.None;
                                }
                            }
                        }
                    }
                    // Running strategies
                    else
                    {
                        if (isGroup)
                        {
                            foreach (var strategyInGroup in item.StrategyInGroup)
                            {
                                bool doRequest = noDelete = strategyInGroup.Position != 0;
                                var agentToDelete = Instance.AgentConnnectionManager.Strategies.FirstOrDefault(it => it.ActualStrategyRunning.Name == strategyInGroup.Name);

                                if (doRequest)
                                {
                                    var form = new GroupAdditionDeleteMode(item.Name.ToString());
                                    form.ShowDialog();
                                    var selectedMode = form.SelectedDeleteMode;
                                    var agent = Instance.AgentsStorage.FirstOrDefault(a => a.Name == strategyInGroup.Name);
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                    {
                                        if (agentToDelete != null)
                                        {
                                            agentToDelete.CloseState = StrategyCloseState.NoWait;
                                        }
                                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                        strat.CheckPosExit();
                                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                        //MainWindow.Instance.DelAgentConfigBtnClick(agent, "has been excluded from the group");
                                    }
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                    {
                                        if (agentToDelete != null)
                                        {
                                            agentToDelete.CloseState = StrategyCloseState.Wait;
                                        }
                                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                        strat.CheckPosWaitStrExit();
                                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                        //MainWindow.Instance.DelAgentConfigBtnClick(agent, "has been excluded from the group");
                                    }
                                    //doDelete = !form.IsCancelled;
                                }
                                else
                                {
                                    if (agentToDelete != null)
                                    {
                                        agentToDelete.CloseState = StrategyCloseState.None;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool doRequest = noDelete = item.SingleAgentPosition != 0;

                            var agentToDelete = Instance.AgentConnnectionManager.Strategies.FirstOrDefault(it => it.ActualStrategyRunning.Name == item.Name);
                            if (doRequest)
                            {
                                var form = new GroupAdditionDeleteMode(item.Name.ToString());
                                form.ShowDialog();
                                var selectedMode = form.SelectedDeleteMode;
                                var agent = Instance.AgentsStorage.FirstOrDefault(a => a.Name == item.Alias);
                                if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                {
                                    if (agentToDelete != null)
                                    {
                                        agentToDelete.CloseState = StrategyCloseState.NoWait;
                                    }
                                    ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                    strat.CheckPosExit();
                                    MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                    MainWindow.Instance.DelAgentConfigBtnClick(agent, "has been excluded from the group");
                                }
                                if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                {
                                    if (agentToDelete != null)
                                    {
                                        agentToDelete.CloseState = StrategyCloseState.Wait;
                                    }
                                    ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                    strat.CheckPosWaitStrExit();
                                    MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                    MainWindow.Instance.DelAgentConfigBtnClick(agent, "has been excluded from the group");
                                }
                                noDelete = !form.IsCancelled;
                            }
                            else
                            {
                                if (agentToDelete != null)
                                {
                                    agentToDelete.CloseState = StrategyCloseState.None;
                                }
                            }
                        }
                    }

                    try
                    {
                        if (!noDelete)
                        {
                            AgentManagerStorage.Remove(item);
                            Task.Run(() => Logger.Info("Agent manager item \"{0}\"/\"{1}\" has been deleted", item.Name, item.Alias));
                            SaveAgentManagerSettings();
                        }
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

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;

            if (null == itemsSource)
            {
                yield return null;
            }

            //foreach (var item in itemsSource)
            for (int i = 0, n = grid.Items.Count; i < n; ++i)
            {
                var row = grid.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;

                if (null != row)
                {
                    yield return row;
                }
            }
        }

        public void ResetStarted()
        {
            if (startStopStartedIndexes == null)
            {
                startStopStartedIndexes = new bool[AgentManagerStorage.Count];
            }
            if (startStopStartedIndexes.Length != AgentManagerStorage.Count)
            {
                bool[] current = new bool[startStopStartedIndexes.Length];
                for (int i = 0, n = Math.Min(startStopStartedIndexes.Length, AgentManagerStorage.Count); i < n; ++i)
                {
                    current[i] = startStopStartedIndexes[i];
                }
                startStopStartedIndexes = new bool[AgentManagerStorage.Count];
                for (int i = 0, n = current.Length; i < n; ++i)
                {
                    startStopStartedIndexes[i] = current[i];
                }
                for (int i = current.Length, n = AgentManagerStorage.Count; i < n; ++i)
                {
                    startStopStartedIndexes[i] = false;
                }
            }

            for (int i = 0, n = AgentManagerListView.Items.Count; i < n; ++i)
            {
                if (startStopStartedIndexes[i])
                {
                    AgentManagerStorage[i].AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Stop;
                    AgentManagerStorage[i].AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Running;
                }
            }
        }

        public void UpdateAgentManagerListView()
        {
            AgentManagerListView.ItemsSource = AgentManagerStorage;
            AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
            AgentManagerCollectionView.Refresh();

            ResetStarted();
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

            EditAgentManagerBtn.IsEnabled = AgentManagerListView.Items.Count != 0;
            DelAgentManagerBtn.IsEnabled = AgentManagerListView.Items.Count != 0 && ((sender as DataGrid).Items.Count > AgentManagerListView.SelectedIndex && ((Common.Entities.AgentManager)(((sender as DataGrid).Items[AgentManagerListView.SelectedIndex]))).AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Stopped);
        }

        private void TestStrategyStartBtnClick(object sender, RoutedEventArgs e)
        {
        }

        private void AgentManagerListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditAgentManagerBtn.IsEnabled = AgentManagerListView.Items.Count != 0;
            DelAgentManagerBtn.IsEnabled = AgentManagerListView.Items.Count != 0 && ((sender as DataGrid).Items.Count > AgentManagerListView.SelectedIndex && AgentManagerListView.SelectedIndex >= 0 && ((Common.Entities.AgentManager)(((sender as DataGrid).Items[AgentManagerListView.SelectedIndex]))).AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Stopped);
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
            var groupElements = Instance.AgentsStorage.Where(i => i.Params.GroupName == agentOrGroup.AgentManagerSettings.AgentOrGroup).ToList();
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

                        var secG = realConnection.Securities.FirstOrDefault(i => i.Id == groupMember.Params.Security);

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
                    
                    var history = new List<long>();
                    var historyAgent = agentOrGroup.StrategyInGroup.FirstOrDefault(i=> i.Name == groupMember.Name);
                    if (historyAgent.MyTradesHistory != null && historyAgent.MyTradesHistory.Count >= 1)
                    {
                        foreach (var t in historyAgent.MyTradesHistory)
                        {
                            if (history.Count == 0)
                            {
                                history.Add( t.Order.TransactionId);
                            }
                            else if (history.Last() != t.Order.TransactionId)
                            {
                                history.Add(t.Order.TransactionId);
                            }
                        } 
                    }

                    string nameGroup = agentOrGroup.ToString();
                    var alias = agentOrGroup.Alias;
                    var port = agentOrGroup.AgentManagerSettings.Portfolio.Name;
                    string[] infoStrategy = { alias, port, nameGroup };
                    strategy = new Strategy();
                    strategy = (Strategy)Activator.CreateInstance(strategyType, groupMember.Params.SettingsStorage, infoStrategy, history);

                    Strategies.Add(strategy);

                    strategy.DisposeOnStop = true;

                    //тест

                    var securityGA = realConnection.Securities.FirstOrDefault(i => i.Id == groupMember.Params.Security);

                    var secutityG = realConnection.Securities.FirstOrDefault(i => i.Code == agentOrGroup.Tool/* agentOrGroup.AgentManagerSettings.Tool*/);
                    if (!string.IsNullOrEmpty(groupMember.Params.Security))
                    {
                        secutityG = realConnection.Securities.FirstOrDefault(i => i.Code == agentOrGroup.Tool/* groupMember.Params.Security*/);
                    }
                    strategy.Security = securityGA;
                    strategy.Portfolio = realConnection.Portfolios.FirstOrDefault(i => i.Name == agentOrGroup.AgentManagerSettings.Portfolio.Code);
                    strategy.Connector = realConnection;
                    strategy.Volume = (decimal)calculatedAmount;
                    var candleManager = new CandleManager(realConnection);
                    strategy.SetCandleManager(candleManager);
                    strategy.LogLevel = LogLevels.Debug;
                    strategy.Start();


                    Strategies.Last().NewMyTrades += trades =>
                    {
                        SaveAgentData(trades, infoStrategy, groupMember.Name);
                    };

                    Strategies.Last().PositionChanged += () =>
                    {
                        UpdatePosition(infoStrategy, groupMember.Name);
                    };
                    // Логирование внутренних событий стратегии для тестов

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

                strategy = new Strategy();

                var history = new List<long>();
                if (agentOrGroup.SingleMyTradesHistory != null && agentOrGroup.SingleMyTradesHistory.Count >= 1)
                { 
                    //history = agentOrGroup.SingleAgentHistory;
                    foreach (var t in agentOrGroup.SingleMyTradesHistory)
                    {
                        if (history.Count == 0)
                        {
                            history.Add( t.Order.TransactionId);
                        }
                        else if (history.Last() != t.Order.TransactionId)
                        {
                           history.Add((long) t.Order.TransactionId);
                        }
                    }
                }

                var nameGroup = "single";
                var alias = agentOrGroup.Alias;
                var port = agentOrGroup.AgentManagerSettings.Portfolio.Name;
                var closeState = agentOrGroup.CloseState.ToString();
                string[] infoStrategy = { alias, port, nameGroup, closeState};

                
                strategy = (Strategy)Activator.CreateInstance(strategyType, agentSetting, infoStrategy, history);

                Strategies.Add(strategy);
                
                
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

                
                 
                Strategies.Last().NewMyTrades += trades =>
                {
                    SaveAgentData(trades, infoStrategy, agentName);
                };

                Strategies.Last().PositionChanged += () =>
                {
                    UpdatePosition(infoStrategy, agentName);
                };

                Strategies.Last().PnLChanged += () =>
                {
                    UpdateMarginData(infoStrategy);
                };


                //Strategies.Last().SecurityChanged += /*(security, pairs, arg3, arg4)*/() =>
                //{

                //    //var sec = security;
                //    //if (sec.Code != "SiZ6")
                //    //    return;
                    

                //    //UpdateSecurityData(infoStrategy, sec.ClosePrice.Value);
                //};
                 
                // Логирование внутренних событий стратегии для тестов

                var wrapper = new AistTraderAgentManagerWrapper(agentOrGroup.Alias, strategy);
                AgentConnnectionManager.Add(wrapper);
            }
        }

         
        

        public void UpdateSecurityData(string[] info, decimal close)
        {
            var agentAlias = info[0];
            var agentGroup = info[2];
            if (agentGroup == "single")
            {
                var actualTime = DateTime.Now;

                var actualStrategy = new ChStrategy();
                foreach (var strategyact in Strategies.Select(st => st as ChStrategy).Where(strategyact2 => strategyact2.Alias == agentAlias))
                {
                    actualStrategy = strategyact;
                }
                

                var actualStrategyData =
                    AgentManagerStorage.Single(i => i.AgentManagerUniqueId == actualStrategy.Alias);
                
                    actualStrategyData.AgentManagerSettings.СurrentPrice = close;

                AgentManagerListView.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    AgentManagerListView.ItemsSource = AgentManagerStorage;
                    AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
                    AgentManagerCollectionView.Refresh();

                   // ResetStarted();

                }));

                //SaveAgentManagerSettings();
            }
            }
        
        public void UpdateMarginData( string[] info)
        {
            var agentAlias = info[0];
            var agentGroup = info[2];
                
            if (agentGroup == "single")
            {
                var actualStrategy = new ChStrategy();
                foreach (var strategyact in Strategies.Select(st => st as ChStrategy).Where(strategyact2 => strategyact2.Alias == agentAlias))
                {
                    actualStrategy = strategyact;
                }
                 
                var actualStrategyData =
                    AgentManagerStorage.Single(i => i.AgentManagerUniqueId == actualStrategy.Alias);

                if (actualStrategyData.AgentManagerSettings.TotalMarginList == null)
                {
                    actualStrategyData.AgentManagerSettings.TotalMarginList = new List<decimal>();
                }
                actualStrategyData.AgentManagerSettings.TotalMarginList.Add(actualStrategy.PnL);

                //if (actualStrategy.PnL == 0)
                //    return;
                if (actualStrategyData.AgentManagerSettings.CurrentMargin !=  actualStrategy.PnL)
                {
                    actualStrategyData.AgentManagerSettings.CurrentMargin =  actualStrategy.PnL;
                    

                    actualStrategyData.AgentManagerSettings.TotalMargin =
                        actualStrategyData.AgentManagerSettings.TotalMarginList.Sum(i => i);
                    AgentManagerListView.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        AgentManagerListView.ItemsSource = AgentManagerStorage;
                        AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
                        AgentManagerCollectionView.Refresh();

                        ResetStarted();

                    }));

                    //SaveAgentManagerSettings();
                }
            }

        }


        public void UpdatePosition( string[] info, string name)
        {
            
            var agentAlias = info[0];
            var agentGroup = info[2];
            if (agentGroup == "single")
            {
                var actualStrategy = new ChStrategy();
                foreach (var strategyact in Strategies.Select(st => st as ChStrategy).Where(strategyact2 => strategyact2.Alias == agentAlias))
                {
                    actualStrategy = strategyact;
                }

                var actualStrategyData =
                    AgentManagerStorage.Single(i => i.AgentManagerUniqueId == actualStrategy.Alias);
                if (actualStrategyData.AgentManagerSettings.Position != (int)actualStrategy.Position)
                {
                    actualStrategyData.AgentManagerSettings.Position = (int)actualStrategy.Position;
                    actualStrategyData.SingleAgentPosition = (int)actualStrategy.Position;
                    if (actualStrategy.Position == 0)
                    {
                        actualStrategyData.AgentManagerSettings.CurrentMargin = 0;
                        actualStrategyData.AgentManagerSettings.TradeEntryPrice = 0;
                    }
                    IConnector connect =  ConnectionManager.Connections.FirstOrDefault(  i => i.ConnectionName == actualStrategyData.AgentManagerSettings.Portfolio.Connection.DisplayName);
                    actualStrategyData.AgentManagerSettings.TradeEntryPrice = actualStrategy.Orders.Last().GetAveragePrice(connect);
                    AgentManagerListView.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        AgentManagerListView.ItemsSource = AgentManagerStorage;
                        AgentManagerCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
                        AgentManagerCollectionView.Refresh();

                        ResetStarted();

                    }));

                    SaveAgentManagerSettings();
                }
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

            var alias = agentManagerToStartAfterEdit.Alias;
            var port = portfolio.Name;
            string[] infoStrategy = { alias, port, nameGroup };

            strategy = new Strategy();
            strategy = (Strategy)Activator.CreateInstance(strategyType, agentSetting, infoStrategy, history);


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
            if (startStopStartedIndexes == null)
            {
                ResetStarted();
            }
            var pressedButton = sender as Button;
            if (startStopStartedIndexes[AgentManagerListView.SelectedIndex] = (pressedButton.Content.ToString() == ManagerParams.AgentManagerOperationCommand.Start.ToString()))
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
                            ChStrategy agentHistory =  agent.ActualStrategyRunning as ChStrategy;
                            var themIDs = agentHistory.TransactionIDs;
                            var agentManagerStorage = Instance.AgentManagerStorage.Single(i => i.Alias == agent.AgentOrGroupName);

                            if (themIDs.Count > 0)
                            {
                                foreach (var t in agentManagerStorage.StrategyInGroup.Where(t => agentHistory.Name == t.Name))
                                {
                                    t.TransactionIdHistory.AddRange(themIDs);
                                }
                            }

                            foreach (var t in agentManagerStorage.StrategyInGroup.Where(t => agentHistory.Name == t.Name))
                            {
                                t.Position = (int)agentHistory.Position;
                            }

                            Task.Run(() => Logger.Info("Stopping - \"{0}\"..", agent.ActualStrategyRunning.Name));
                            agent.ActualStrategyRunning.Stop();
                            var test = agent.ActualStrategyRunning as ChStrategy;
                            var agentInColl = MainWindow.Instance.AgentsStorage.FirstOrDefault(i => i.Name== agent.ActualStrategyRunning.Name);
                            AgentConnnectionManager.Strategies.Remove(agent);
                        }

                        item.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Start;
                        item.AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Stopped;

                        SaveAgentManagerSettings();
                    }
                }
                else
                {
                    var strategyOrGroup = AgentConnnectionManager.Strategies.FirstOrDefault(i => i.AgentOrGroupName == item.Alias) as AistTraderAgentManagerWrapper;
                    ChStrategy agent =  strategyOrGroup.ActualStrategyRunning as ChStrategy;
                    var themIDs = agent.TransactionIDs;
                    var agentManagerStorage = Instance.AgentManagerStorage.Single(i => i.Alias == strategyOrGroup.AgentOrGroupName);

                    if (themIDs.Count > 0)
                    {
                        if (agentManagerStorage.SingleAgentHistory == null)
                        {
                            agentManagerStorage.SingleAgentHistory = new List<long>();
                        }
                        agentManagerStorage.SingleAgentHistory.AddRange(themIDs);
                    }

                    agentManagerStorage.SingleAgentPosition = (int) agent.Position;


                    strategyOrGroup.ActualStrategyRunning.Stop();
                    AgentConnnectionManager.Strategies.Remove(strategyOrGroup);
                    item.AgentManagerSettings.Command = ManagerParams.AgentManagerOperationCommand.Start; ;
                    item.AgentManagerSettings.AgentMangerCurrentStatus =ManagerParams.AgentManagerStatus.Stopped; ;
                }
                SaveAgentManagerSettings();
                UpdateAgentManagerListView();
            }
            DelAgentManagerBtn.IsEnabled = strategy.ProcessState != StockSharp.Algo.ProcessStates.Started && strategy.ProcessState != StockSharp.Algo.ProcessStates.Stopping;
        }

        public void SaveAgentData(IEnumerable<MyTrade> trades , string[] info, string nameStrategy)
        {
            var agentAlias = info[0];
            var agentGroup = info[2];

            var agentManagerStorage = Instance.AgentManagerStorage;

            if (agentGroup == "single")
            {
                var agentManagerStorageSingle = agentManagerStorage.FirstOrDefault(i => i.AgentManagerUniqueId == agentAlias);
                if (agentManagerStorageSingle.SingleMyTradesHistory == null)
                {
                    agentManagerStorageSingle.SingleMyTradesHistory = new List<MyTrade>();
                }

                agentManagerStorageSingle.SingleMyTradesHistory.AddRange(trades);
            }
            else
            {
                var agentManagerStorageGroup = agentManagerStorage.FirstOrDefault(i => i.AgentManagerUniqueId == agentAlias);
                var strategyStorage =  agentManagerStorageGroup.StrategyInGroup.FirstOrDefault(i => i.Name == nameStrategy);
                if (strategyStorage.MyTradesHistory == null)
                {
                    strategyStorage.MyTradesHistory = new List<MyTrade>();
                }

                strategyStorage.MyTradesHistory.AddRange(trades);
            }
            
            SaveAgentManagerSettings();
        } 

        #region Aist Trader Agent/Group Manager

        public class AistTraderAgentManagerWrapper
        {
            private StrategyCloseState strategyCloseState;

            public AistTraderAgentManagerWrapper(string name, Strategy strategy)
            {
                AgentOrGroupName = name;
                ActualStrategyRunning = strategy;
                strategyCloseState = StrategyCloseState.None;
            }

            public override string ToString()
            {
                return AgentOrGroupName;
            }

            public Strategy ActualStrategyRunning { get; set; }

            public StrategyCloseState CloseState
            {
                get
                {
                    return strategyCloseState;
                }
                set
                {
                    strategyCloseState = value;
                    var agentManager = MainWindow.Instance.AgentManagerStorage.FirstOrDefault(a => a.Name == AgentOrGroupName);
                    if (agentManager != null)
                    {
                        agentManager.CloseState = strategyCloseState;
                        if (agentManager.StrategyInGroup != null && agentManager.StrategyInGroup.Count > 0)
                        {
                            var strategyInGroup = agentManager.StrategyInGroup.FirstOrDefault(s => s.Name == AgentOrGroupName);
                            if (strategyInGroup != null)
                            {
                                strategyInGroup.CloseState = strategyCloseState;
                            }
                        }
                    }
                }
            }
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
