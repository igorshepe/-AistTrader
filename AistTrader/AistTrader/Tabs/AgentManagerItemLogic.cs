using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml.Serialization;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using NLog;

namespace AistTrader
{
    public partial class MainWindow
    {
        public bool IsAgentManagerSettingsLoaded;
        private void AddAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new AgentManagerAddition();
            form.ShowDialog();
            form = null;
            SaveAgentManagerSettings();
        }
        public void DelAgentBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in AgentManagerListView.SelectedItems.Cast<AgentManager>().ToList())
            {
                AgentManagerStorage.Remove(item);
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
                                          select new AgentManagerAddition(agentConfigs, index))
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
                AgentManagerStorage.Add(settings);
            SaveAgentManagerSettings();
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
        }
    }
}
