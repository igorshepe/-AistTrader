using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using AistTrader.Properties;
using Common.Entities;

namespace AistTrader
{
    public partial class MainWindow
    {
        public ObservableCollection<AgentManager> AgentManagerStorage { get; private set; }
        public CollectionView AgentManagerView { get; set; }


        private void LoadAgentManagerTabItemData()
        {
            AgentManagerStorage = new ObservableCollection<AgentManager>();
            LoadAgentManagerSettings();
        }

        private void AddAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {

            var form = new AgentManagerAddition();
            form.ShowDialog();
            form = null;
            SaveAgentSettings();
        }
        public void DelAgentBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in AgentManagerListView.SelectedItems.Cast<AgentManager>().ToList())
            {
                AgentManagerStorage.Remove(item);
            }
            //SaveSettings();
        }
        private void LoadAgentManagerSettings()
        {
            if (Settings.Default.AgentManager == null) return;
            try
            {
                foreach (var rs in Settings.Default.AgentManager.Cast<AgentManager>())
                {
                    AgentManagerStorage.Add(rs);
                }
                AgentManagerListView.ItemsSource = AgentManagerStorage;
                AgentManagerView = (CollectionView)CollectionViewSource.GetDefaultView(AgentManagerListView.ItemsSource);
                AgentManagerView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
            }
            catch (Exception)
            {
                MessageBox.Show(this, @"Не удалось прочитать настройки. Задайте заново.");
                Settings.Default.AgentManager.Clear();
            }
        }


        private void EditAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = AgentManagerListView.SelectedItems.Cast<AgentManager>().ToList();
            foreach (var addQuikWindow in from agentConfigs in listToEdit
                                          let index = AgentManagerStorage.IndexOf(agentConfigs)
                                          where index != -1
                                          select new AgentManagerAddition(agentConfigs, index))
            {
                addQuikWindow.Title = "Редактирование конфигурации";
                addQuikWindow.ShowDialog();
                //SaveSettings();
            }

        }



    }
}
