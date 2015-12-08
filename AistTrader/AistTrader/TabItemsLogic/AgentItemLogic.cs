using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Ecng.Common;

namespace AistTrader
{
    public partial class MainWindow
    {
        public CollectionView AgentCollectionView { get; set; }
        public ObservableCollection<Agent> AgentsStorage { get; private set; }

        private void AgentListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = AgentListView.SelectedItem as Agent;
            if (item != null && item._Agent.GroupName == "Without Group")
                EditSingleOrGroupItemBtn.IsEnabled = false;
            else
                EditSingleOrGroupItemBtn.IsEnabled = true;

            if (AgentsStorage.Count == 0)
                EditSingleOrGroupItemBtn.IsEnabled = false;
        }
        private void AgentListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (AgentsStorage.Count > 0)
                EditSingleOrGroupItemBtn.IsEnabled = true;
            else
                EditSingleOrGroupItemBtn.IsEnabled = false;
        }
        private void AddAgentConfigGroupBtnClick(object sender, RoutedEventArgs e)
        {
            new AgentGroupAddition().ShowDialog();
            SaveAgentSettings();
        }
        private void AddAgentConfigBtnClick(object sender, RoutedEventArgs e)
        {

            //AgentConfigView newAgentConfigView = new AgentConfigView();
            //AistTrader.Models.AgentConfigModel newConfigModel = new Models.AgentConfigModel();
            //newAgentConfigView.DataContext = new ViewModels.AgentConfigViewModel(newConfigModel);
            //newAgentConfigView.Show();



            var form = new AgentConfig();
            form.ShowDialog();
            form = null;
            SaveAgentSettings();
            
            
            //var form = new AgentConfig();
            //form.ShowDialog();
            //form = null;
            //SaveAgentSettings();
        }
        private void SaveAgentSettings()
        {
            var agentSettings = AgentsStorage.OrderBy(s => "{0}-{1}".Put(s.Name, s._Agent.ToString())).ToList();
            Settings.Default.Agents = new SettingsArrayList(agentSettings);
            Settings.Default.Save();
        }

        public void DeleteAgentBtnClick(object sender, RoutedEventArgs e)
        {
            var items = AgentListView.SelectedItems.Cast<Agent>().ToList();
            foreach (var i in items)
            {
                var agent = i;
                if (agent._Agent.GroupName != "Without Group")
                {
                    foreach (var item in AgentListView.SelectedItems.Cast<Agent>().ToList())
                    {
                        AgentsStorage.Remove(item);
                    }
                    SaveAgentSettings();
                }
                else
                {
                    var result = AgentListView.Items.Cast<Agent>().ToList();
                    var isUsedinAnyOtherGroup = result.Where(a => a.Name == agent.Name && a._Agent.GroupName != "Without Group").Select(a => a).Any();
                    if (isUsedinAnyOtherGroup)
                    {
                        MessageBox.Show("Нельзя удалить, используется в группе");
                    }
                    else
                    {
                        foreach (var item in AgentListView.SelectedItems.Cast<Agent>().ToList())
                        {
                            AgentsStorage.Remove(item);
                        }
                        SaveAgentSettings();
                    }
                }
            }
        }

        private void LoadAgentTabItemData()
        {
            AgentsStorage = new ObservableCollection<Agent>();
            AgentsStorage.CollectionChanged += AgentSettingsStorageChanged;
            LoadAgentSettings();
        }

        public void DelAgentConfigBtnClick(Agent agent)
        {
            AgentsStorage.Remove(agent);
            SaveAgentSettings();
        }
        private void AgentSettingsStorageChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(AgentListView.View is GridView))
                return;
            var gridView = AgentListView.View as GridView;
            foreach (var column in gridView.Columns)
            {
                //ResizeGridViewColumn(column);
            }
            if (AgentsStorage.Count > 1)
                CreateGroupItemBtn.IsEnabled = true;
            else
                CreateGroupItemBtn.IsEnabled = false;
        }

        private void EditAgentConfigBtnClick(object sender, RoutedEventArgs e)
        {
            if (AgentListView.SelectedItem == null)
                return;
            var items = AgentListView.SelectedItems.Cast<Agent>().ToList();
            var item = items.Where(a => a._Agent.GroupName == "Without Group").Select(a => a).Any();
            if (item)
            {
                var listToEdit = AgentListView.SelectedItems.Cast<Agent>().ToList();
                foreach (var addQuikWindow in from agentConfigs in listToEdit
                                              let index = AgentsStorage.IndexOf(agentConfigs)
                                              where index != -1
                                              select new AgentConfig(agentConfigs, index))
                {
                    addQuikWindow.Title = "Редактирование конфигурации";
                    addQuikWindow.ShowDialog();
                    SaveAgentSettings();
                }
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Данная конфигурация находися в группе, редактировать всю группу?", "Редактирование", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var listToEdit = AgentListView.SelectedItems.Cast<Agent>().ToList();
                    foreach (var addQuikWindow in from agentConfigs in listToEdit
                                                  let index = AgentsStorage.IndexOf(agentConfigs)
                                                  where index != -1
                                                  select new AgentGroupAddition(agentConfigs, index, AgentWorkMode.Group))
                    {
                        addQuikWindow.Title = "Редактирование конфигурации";
                        addQuikWindow.ShowDialog();
                        SaveAgentSettings();
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    var listToEdit = AgentListView.SelectedItems.Cast<Agent>().ToList();
                    foreach (var addQuikWindow in from agentConfigs in listToEdit
                                                  let index = AgentsStorage.IndexOf(agentConfigs)
                                                  where index != -1
                                                  select new AgentGroupAddition(agentConfigs, index, AgentWorkMode.Single))
                    {
                        addQuikWindow.Title = "Редактирование конфигурации";
                        addQuikWindow.ShowDialog();
                        SaveAgentSettings();
                    }
                }
            }
        }

        public void AddNewAgent(Agent settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentsStorage.Count)
                AgentsStorage[editIndex] = settings;
            else
                AgentsStorage.Add(settings);

            
        }

        public void LoadAgentSettings()
        {
            if (Settings.Default.Agents == null) return;
            try
            {
                foreach (var rs in Settings.Default.Agents.Cast<Agent>())
                {
                    AgentsStorage.Add(rs);
                }
                AgentListView.ItemsSource = AgentsStorage;


                AgentCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentListView.ItemsSource);
                AgentCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("_Agent.GroupName"));
            }
            catch (Exception)
            {
                MessageBox.Show(this, @"Не удалось прочитать настройки. Задайте заново.");
                Settings.Default.Robots.Clear();
            }
        }

        private void OperationBtnClick(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
