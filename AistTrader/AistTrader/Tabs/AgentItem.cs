using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using Strategies.Strategies;
using NLog;

namespace AistTrader
{
    public partial class MainWindow
    {
        public bool AllAgentsChecked { get; set; }
        public bool IsAgentSettingsLoaded;
        public bool IsGroupWritten;
        private void AgentListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = AgentListView.SelectedItem as Agent;

            if (item != null && item.Params.GroupName == "ungrouped agents")
            {
                EditSingleOrGroupItemBtn.IsEnabled = false;
                EditSingleOrGroupItemBtn.ToolTip = "Added agents can't be edited.";
            }
            else
            {
                EditSingleOrGroupItemBtn.IsEnabled = true;
            }

            if (AgentsStorage.Count == 0)
            {
                EditSingleOrGroupItemBtn.IsEnabled = false;
                EditSingleOrGroupItemBtn.ToolTip = "Added agents can't be edited.";
            }
            if (AgentListView.Items.Count == 0)
            {
                DelAgentBtn.IsEnabled = false;
                DelAgentBtn.ToolTip = "No agents to delete";
                return;
            }
            else
            {
                DelAgentBtn.IsEnabled = true;
            }
        }

        private void AgentListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsAgentSettingsLoaded & (File.Exists("Agents.xml")))
            {
                InitiateAgentSettings();
            }
            var selectedItem = AgentListView.SelectedItem as Agent;
            if (selectedItem != null && (AgentsStorage.Count > 0 && selectedItem.Params.GroupName != "ungrouped agents"))
            {
                EditSingleOrGroupItemBtn.IsEnabled = true;
            }
            else
            {
                EditSingleOrGroupItemBtn.IsEnabled = false;
            }

            if (AgentListView.Items.Count == 0)
            {
                DelAgentBtn.IsEnabled = false;
            }
            else
            {
                DelAgentBtn.IsEnabled = true;
            }
        }

        private void AddAgentConfigGroupBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new GroupAddition().ShowDialog();
            form = null;
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
                    DataContractSerializer ser = new DataContractSerializer(typeof(List<Agent>));
                    ser.WriteObject(fStream, obj);
                    fStream.Close();
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => Logger.Log(LogLevel.Error, ex.Message));
            }
        }

        public void InitiateAgentSettings()
        {
            using (FileStream fs = new FileStream("Agents.xml", FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var xmlSerializer = new DataContractSerializer(typeof(List<Agent>), new Type[] { typeof(Agent) });
                    var agents = (List<Agent>)xmlSerializer.ReadObject(fs);
                    fs.Close();
                    if (agents == null) { return; }

                    AgentsStorage.Clear();
                    foreach (var rs in agents)
                    {
                        AgentsStorage.Add(rs);
                    }
                    AgentListView.ItemsSource = AgentsStorage;
                    AgentCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentListView.ItemsSource);
                    if (AgentCollectionView.GroupDescriptions != null && AgentCollectionView.GroupDescriptions.Count == 0)
                    {
                        AgentCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Params.GroupName"));
                    }
                    IsAgentSettingsLoaded = true;
                }
                catch (Exception e)
                {
                    IsAgentSettingsLoaded = false;
                    fs.Close();
                    Task.Run(() => Logger.Log(LogLevel.Error, e.Message));
                    Task.Run(() => Logger.Log(LogLevel.Error, e.InnerException.Message));
                    if (e.InnerException.Message == "Root element is missing.")
                    {
                        IsAgentSettingsLoaded = false;
                    }
                }
            }
        }

        public void DeleteAgentBtnClick(object sender, RoutedEventArgs e)
        {
            if (AllAgentsChecked)
            {
                MessageBoxResult result = MessageBox.Show("All agents will be deleted! Confirm?", "Delete all agents", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    var delList = AgentListView.Items.Cast<Agent>().Select(r => r).ToList();
                    foreach (var i in delList)
                    {
                        AgentsStorage.Remove(i);
                    }
                    Task.Run(() => Logger.Info("All agents have been deleted."));
                    SaveAgentSettings();
                    AllAgentsChecked = false;
                }
                else
                {
                    return;
                }
            }
            if (AgentListView.Items.Cast<Agent>().Count(i => i.Params.IsChecked) > 1)
            {
                var delList = AgentListView.Items.Cast<Agent>().Where(i => i.Params.IsChecked).ToList();
                foreach (var i in delList)
                {
                    //TODO: в вопросы, после ответа доделать функционал
                    AgentsStorage.Remove(i);
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
                        MessageBoxResult result = MessageBox.Show("Single group member can not be deleted, delete the whole group?", "Delete group", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                        if (result == MessageBoxResult.Yes)
                        {
                            var isUsedInAgentManager = AgentManagerStorage.Any(am => am.AgentManagerSettings.AgentOrGroup == agent.Params.GroupName.ToString());
                            if (isUsedInAgentManager)
                            {
                                MessageBox.Show("Group - \"{0}\" can not be deleted, used in agent manager".Put(agent.Params.GroupName));
                                return;
                            }
                            var delList = AgentListView.Items.Cast<Agent>().Where(r => r.Params.GroupName == agent.Params.GroupName).ToList();
                            foreach (var del in delList)
                            {
                                var agentToDelete = MainWindow.Instance.AgentConnnectionManager.Strategies.FirstOrDefault(it => it.ActualStrategyRunning.Name == del.Name);

                                var strategyOrGroup = AgentConnnectionManager.Strategies.FirstOrDefault(inst => inst.AgentOrGroupName == del.Name) as AistTraderAgentManagerWrapper;
                                if (Instance.AgentManagerStorage.Any(inst => inst.Alias == strategyOrGroup.AgentOrGroupName) &&
                                    Instance.AgentManagerStorage.Single(inst => inst.Alias == strategyOrGroup.AgentOrGroupName).SingleAgentPosition != 0)
                                {
                                    var form = new GroupAdditionDeleteMode(del.Name.ToString());
                                    form.ShowDialog();
                                    var selectedMode = form.SelectedDeleteMode;
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                    {
                                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                        strat.CheckPosExit();
                                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                        MainWindow.Instance.DelAgentConfigBtnClick(del, "has been excluded from the group");
                                    }
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                    {
                                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                        strat.CheckPosWaitStrExit();
                                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                        MainWindow.Instance.DelAgentConfigBtnClick(del, "has been excluded from the group");
                                    }
                                }

                                AgentsStorage.Remove(del);
                            }
                            Task.Run(() => Logger.Info("Group - \"{0}\" has been deleted.", agent.Params.GroupName));
                            SaveAgentSettings();
                            AllAgentsChecked = false;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        var result = AgentListView.Items.Cast<Agent>().ToList();
                        var isUsedinAnyOtherGroup = result.Where(a => a.Name == agent.Name && a.Params.GroupName != "ungrouped agents").Select(a => a).Any();
                        if (isUsedinAnyOtherGroup)
                        {
                            MessageBox.Show("Can not be deleted, used in a group");
                            return;
                        }
                        var agentItem = AgentListView.SelectedItem as Agent;

                        var isUsedInAgentManager = AgentManagerStorage.Any(am => am.AgentManagerSettings.AgentOrGroup == agentItem.Params.FriendlyName.ToString());
                        if (isUsedInAgentManager)
                        {
                            MessageBox.Show("Can not be deleted, used in agent manager");
                            return;
                        }
                        else
                        {
                            foreach (var item in AgentListView.SelectedItems.Cast<Agent>().ToList())
                            {
                                MessageBoxResult resultMsg = MessageBox.Show("Selected agent will be permanently deleted! Confirm?", "Delete agent", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);

                                var agentToDelete = MainWindow.Instance.AgentConnnectionManager.Strategies.FirstOrDefault(it => it.ActualStrategyRunning.Name == item.Name);

                                var strategyOrGroup = AgentConnnectionManager.Strategies.FirstOrDefault(inst => inst.AgentOrGroupName == item.Name) as AistTraderAgentManagerWrapper;
                                if (Instance.AgentManagerStorage.Any(inst => inst.Alias == strategyOrGroup.AgentOrGroupName) &&
                                    Instance.AgentManagerStorage.Single(inst => inst.Alias == strategyOrGroup.AgentOrGroupName).SingleAgentPosition != 0)
                                {
                                    var form = new GroupAdditionDeleteMode(item.Name.ToString());
                                    form.ShowDialog();
                                    var selectedMode = form.SelectedDeleteMode;
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                    {
                                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                        strat.CheckPosExit();
                                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                        MainWindow.Instance.DelAgentConfigBtnClick(item, "has been excluded from the group");
                                    }
                                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                    {
                                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                        strat.CheckPosWaitStrExit();
                                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                        MainWindow.Instance.DelAgentConfigBtnClick(item, "has been excluded from the group");
                                    }
                                }

                                if (resultMsg == MessageBoxResult.Yes)
                                {
                                    try
                                    {
                                        AgentsStorage.Remove(item);

                                        for (int j = 0; j < AgentManagerStorage.Count; ++j)
                                        {
                                            if (AgentManagerStorage[j].AgentManagerSettings.AgentOrGroup == item.Name)
                                            {
                                                AgentManagerStorage.Remove(AgentManagerStorage[j]);
                                                --j;
                                            }
                                        }

                                        Task.Run(() => Logger.Info("Agent \"{0}\" has been deleted.  Strategies class name: {1}.cs", item.Params.FriendlyName, item.Name));
                                    }
                                    catch (Exception ex)
                                    {
                                        Task.Run(() => Logger.Log(LogLevel.Error, ex.Message));
                                    }
                                }
                            }
                        }
                        SaveAgentSettings();
                    }
                }
            }
        }

        public void DelAgentConfigBtnClick(Agent agent, string msg)
        {
            AgentsStorage.Remove(agent);
            if (msg != null)
            {
                Task.Run(() => Logger.Info("Agent \"{0}\" {1} - \"{2}\" ", agent.Name, msg,agent.Params.GroupName));
            }
            SaveAgentSettings();
        }
        private void AgentSettingsStorageChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AgentsStorage.Count > 1)
            {
                CreateGroupItemBtn.IsEnabled = true;
            }
            else
            {
                CreateGroupItemBtn.IsEnabled = false;
                CreateGroupItemBtn.ToolTip = "Can't create a group with one registred agent";
            }
        }

        private void EditAgentConfigBtnClick(object sender, RoutedEventArgs e)
        {
            if (AgentListView.SelectedItem == null)
            {
                return;
            }
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
                    addQuikWindow.Title = "Edit configuration";
                    addQuikWindow.ShowDialog();
                    SaveAgentSettings();
                }
            }
            else
            {
                var listToEdit = AgentListView.SelectedItems.Cast<Agent>().ToList();
                foreach (var addQuikWindow in from agentConfigs in listToEdit
                                                let index = AgentsStorage.IndexOf(agentConfigs)
                                                where index != -1
                                                select new GroupAddition(agentConfigs, index, AgentWorkMode.Group))
                {
                    addQuikWindow.Title = "Edit configuration";
                    addQuikWindow.ShowDialog();
                    SaveAgentSettings();
                }
            }
        }

        public void AddNewAgent(Agent agent, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentsStorage.Count)
            {
                AgentsStorage[editIndex] = agent;
                //если в членах группы изменился объем
                if (agent.Params.Amount != agent.Params.PhantomParams.Amount)
                {
                    Task.Run(() => Logger.Info("{0} {1}: amount changed, \"{2}\" -> \"{3}\"", agent.Params.GroupName, agent.Name, agent.Params.PhantomParams.Amount, agent.Params.Amount));
                }
            }
            else
            {
                try
                {
                    AgentsStorage.Add(agent);
                    Task.Run(() => Logger.Info("Successfully added agent - \"{0}\"", agent.Params.FriendlyName));
                }
                catch (Exception)
                {
                    Task.Run(() => Logger.Info("Error adding agent - \"{0}\"", agent.Params.FriendlyName));
                }
            }
            SaveAgentSettings();
            UpdateAgentListView();
        }

        public void AddNewAgentInGroup(Agent agent, int editIndex,bool isNewAddition)
        {
            if (editIndex >= 0 && editIndex < AgentsStorage.Count)
            {
                if (!IsGroupWritten & agent.Params.GroupName != agent.Params.PhantomParams.GroupName)
                {
                    Task.Run(() => Logger.Info("Group \"{0}\" has changed it's name to -> \"{1}\"", agent.Params.PhantomParams.GroupName, agent.Params.GroupName));
                    IsGroupWritten = true;
                }
                AgentsStorage[editIndex] = agent;
                if (agent.Params.Amount != agent.Params.PhantomParams.Amount)
                {
                    Task.Run(() => Logger.Info("{0} {1}: amount changed, \"{2}\" -> \"{3}\"", agent.Params.GroupName, agent.Name, agent.Params.PhantomParams.Amount, agent.Params.Amount));
                }
            }
            else
            {
                try
                {
                    AgentsStorage.Add(agent);
                    Task.Run(() => Logger.Info("Agent \"{0}\" successfully added to the group \"{1}\"", agent.Params.FriendlyName, agent.Params.GroupName));
                }
                catch (Exception)
                {
                    Task.Run(() => Logger.Info("Error adding agent - \"{0}\"", agent.Params.FriendlyName));
                }
            }
            SaveAgentSettings();
            UpdateAgentListView();
        }

        public void UpdateAgentListView()
        {
            AgentListView.ItemsSource = AgentsStorage;
            AgentCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(AgentListView.ItemsSource);
            if (AgentCollectionView.GroupDescriptions != null && AgentCollectionView.GroupDescriptions.Count == 0)
            {
                AgentCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Params.GroupName"));
            }
        }
       
        private void ChkBoxSelectAllAgents_OnClick(object sender, RoutedEventArgs e)
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
    }
}
