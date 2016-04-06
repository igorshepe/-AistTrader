using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using Common.Entities;
using NLog;
using Strategies.Strategies;

namespace AistTrader
{
    public partial class MainWindow
    {
        public bool IsAgentManagerSettingsLoaded;
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
                if (AgentManagerCollectionView.GroupDescriptions.Count == 0)
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

            var strategytest = new ChStrategy
            {
                Security = ConnectionManager.Connections[0].Securities.First(i => i.Code == "SiH6"),
                Connector = ConnectionManager.Connections.First()
            };
            strategytest.Start();
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
            var item = (sender as FrameworkElement).DataContext;
            var form = new ManagerAdditionTradeSettings();
            form.ShowDialog();
            form = null;

        }

        private void StartStopBtnClick(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
