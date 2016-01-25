using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using Common.Entities;
using Common.Settings;
using NLog;

namespace AistTrader
{
    public partial class MainWindow
    {
        public bool IsAgentSettingsLoaded;
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
            if (!IsAgentSettingsLoaded && (File.Exists("AgentSettings.xml")))
                InitiateAgentSettings();
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
            var form = new AgentConfig();
            form.ShowDialog();
            form = null;
        }
        private void SaveAgentSettings()
        {
            List<Agent> obj = AgentsStorage.Select(a => a).ToList();
            var fStream = new FileStream("AgentSettings.xml", FileMode.Create, FileAccess.Write, FileShare.None);
            var xmlSerializer = new XmlSerializer(typeof(List<Agent>), new Type[] { typeof(Agent) });
            xmlSerializer.Serialize(fStream, obj);
            fStream.Close();
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
  
        public void DelAgentConfigBtnClick(Agent agent)
        {
            AgentsStorage.Remove(agent);
            SaveAgentSettings();
        }
        private void AgentSettingsStorageChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(AgentsStorage.Count > 1)
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
            SaveAgentSettings();
            UpdateAgentListView();
        }
        public void UpdateAgentListView()
        {
            AgentListView.ItemsSource = AgentsStorage;
            AgentCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentListView.ItemsSource);
        }
        public void InitiateAgentSettings()
         {
            StreamReader sr = new StreamReader("AgentSettings.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<Agent>), new Type[] { typeof(Agent) });
                var agents = (List<Agent>)xmlSerializer.Deserialize(sr);
                sr.Close();
                if (agents == null) return;

                AgentsStorage.Clear();
                foreach (var rs in agents)
                {
                    AgentsStorage.Add(rs);
                }
                AgentListView.ItemsSource = AgentsStorage;
                AgentCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentListView.ItemsSource);
                AgentCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("_Agent.GroupName"));
                IsAgentSettingsLoaded = true;
            }

            catch (Exception e)
            {
                IsAgentSettingsLoaded = false;
                sr.Close();
                Logger.Log(LogLevel.Error,  e.Message);
                Logger.Log(LogLevel.Error, e.InnerException.Message);
                if (e.InnerException.Message == "Root element is missing.")
                    IsAgentSettingsLoaded = false;
            }
        }
    }
}
