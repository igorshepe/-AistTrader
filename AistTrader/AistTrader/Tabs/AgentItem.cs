using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;
using Common.Entities;
using Common.Params;
using Ecng.Collections;
using MoreLinq;
using NLog;

namespace AistTrader
{
    public partial class MainWindow
    {
        public bool AllAgentsChecked { get; set; }
        public bool IsAgentSettingsLoaded;
        private void AgentListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = AgentListView.SelectedItem as Agent;
            if (item != null && item.Params.GroupName == "ungrouped agents")
                EditSingleOrGroupItemBtn.IsEnabled = false;
            else
                EditSingleOrGroupItemBtn.IsEnabled = true;
            if (AgentsStorage.Count == 0)
                EditSingleOrGroupItemBtn.IsEnabled = false;
        }
        private void AgentListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsAgentSettingsLoaded & (File.Exists("Agents.xml")))
                InitiateAgentSettings();
            if (AgentsStorage.Count > 0)
                EditSingleOrGroupItemBtn.IsEnabled = true;
            else
                EditSingleOrGroupItemBtn.IsEnabled = false;
        }
        private void AddAgentConfigGroupBtnClick(object sender, RoutedEventArgs e)
        {
            new GroupAddition().ShowDialog();
            SaveAgentSettings();
        }
        private void AddAgentConfigBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new AgentAddition();
            form.ShowDialog();
            form = null;
        }
        private void SaveAgentSettings()
        {
            try
            {
                List<Agent> obj = AgentsStorage.Select(a => a).ToList();
                using (var fStream = new FileStream("Agents.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var xmlSerializer = new XmlSerializer(typeof(List<Agent>), new Type[] { typeof(Agent) });
                    xmlSerializer.Serialize(fStream, obj);
                    fStream.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }
        public void DeleteAgentBtnClick(object sender, RoutedEventArgs e)
        {
            if (AllAgentsChecked)
            {
                MessageBoxResult result = MessageBox.Show("Будут удалены все агенты! Подтвердить?", "Удаление всех агентов", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var delList = AgentListView.Items.Cast<Agent>().Select(r => r).ToList();
                    foreach (var i in delList)
                        AgentsStorage.Remove(i);
                    Logger.Info("All agents have been deleted.");
                    SaveAgentSettings();
                    AllAgentsChecked = false;
                }
                else
                    return;
            }
            if (AgentListView.Items.Cast<Agent>().Count(i => i.Params.IsChecked) > 1)
            {
                var delList = AgentListView.Items.Cast<Agent>().Where(i => i.Params.IsChecked).ToList();
                foreach (var i in delList)
                {
                    //todo: в вопросы, после ответа доделать функционал

                    //AgentsStorage.Remove(i);
                }
            }
            else
            {
                var items = AgentListView.SelectedItems.Cast<Agent>().ToList();
                foreach (var i in items)
                {
                    var agent = i;
                    if (agent.Params.GroupName != "ungrouped agents")
                    {
                        foreach (var item in AgentListView.SelectedItems.Cast<Agent>().ToList())
                        {
                            try
                            {
                                AgentsStorage.Remove(item);
                                Logger.Info("Agent \"{0}\" has been deleted.  Strategies class name: {1}.cs", item.Params.FriendlyName, item.Name);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Error, ex.Message);
                            }
                        }
                        SaveAgentSettings();
                    }
                    else
                    {
                        var result = AgentListView.Items.Cast<Agent>().ToList();
                        var isUsedinAnyOtherGroup = result.Where(a => a.Name == agent.Name && a.Params.GroupName != "ungrouped agents").Select(a => a).Any();
                        if (isUsedinAnyOtherGroup)
                        {
                            MessageBox.Show("Нельзя удалить, используется в группе");
                        }
                        else
                        {
                            foreach (var item in AgentListView.SelectedItems.Cast<Agent>().ToList())
                            {
                                try
                                {
                                    AgentsStorage.Remove(item);
                                    Logger.Info("Agent \"{0}\" has been deleted.  Strategies class name: {1}.cs", item.Params.FriendlyName, item.Name);

                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(LogLevel.Error, ex.Message);
                                }
                            }
                            SaveAgentSettings();
                        }
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
            var item = items.Where(a => a.Params.GroupName == "ungrouped agents").Select(a => a).Any();
            if (item)
            {
                var listToEdit = AgentListView.SelectedItems.Cast<Agent>().ToList();
                foreach (var addQuikWindow in from agentConfigs in listToEdit
                                              let index = AgentsStorage.IndexOf(agentConfigs)
                                              where index != -1
                                              select new AgentAddition(agentConfigs, index))
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
                                                  select new GroupAddition(agentConfigs, index, AgentWorkMode.Group))
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
                                                  select new GroupAddition(agentConfigs, index, AgentWorkMode.Single))
                    {
                        addQuikWindow.Title = "Редактирование конфигурации";
                        addQuikWindow.ShowDialog();
                        SaveAgentSettings();
                    }
                }
            }
        }
        public void AddNewAgent(Agent agent, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentsStorage.Count)
                AgentsStorage[editIndex] = agent;
            else
                try
                {
                    AgentsStorage.Add(agent);
                    Logger.Info("Successfully added agent - \"{0}\"", agent.Params.FriendlyName);
                }
                catch (Exception)
                {
                    Logger.Info("Error adding agent - \"{0}\"", agent.Params.FriendlyName);
                }
            SaveAgentSettings();
            UpdateAgentListView();
        }
        public void UpdateAgentListView()
        {
            AgentListView.ItemsSource = AgentsStorage;
            AgentCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentListView.ItemsSource);
            if(AgentCollectionView.GroupDescriptions.Count ==0)
                AgentCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Params.GroupName"));
        }
        public void InitiateAgentSettings()
         {
            StreamReader sr = new StreamReader("Agents.xml");
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
                if (AgentCollectionView.GroupDescriptions.Count == 0)
                    AgentCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Params.GroupName"));
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
        private void ChkBoxSelectAllAgents_OnClick(object sender, RoutedEventArgs e)
        {
            if (AllAgentsChecked == true)
            {
                var list = AgentListView.Items.Cast<Agent>().Select(i => i).ToList();
                foreach (var i in list)
                    i.Params.IsChecked = true;
                ICollectionView view = CollectionViewSource.GetDefaultView(AgentListView.Items);
                view.Refresh();
            }
            else
            {
                var list = AgentListView.Items.Cast<Agent>().Select(i => i).ToList();
                foreach (var i in list)
                    i.Params.IsChecked = false;
                ICollectionView view = CollectionViewSource.GetDefaultView(AgentListView.Items);
                view.Refresh();
            }
        }
    }
}
