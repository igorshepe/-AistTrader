using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml.Serialization;
using Common.Entities;
using NLog;

namespace AistTrader
{
    public partial class MainWindow
    {
        
        public bool IsPortfolioSettingsLoaded;

        private void LoadPortfolioTabItemData()
        {
        }

        public void AddNewAgentPortfolio(AgentPortfolio settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentPortfolioStorage.Count)
                AgentPortfolioStorage[editIndex] = settings;
            else
                AgentPortfolioStorage.Add(settings);
            SavePortfolioSettings();

            //UpdatePortfolioListView();
        }

        public void UpdatePortfolioListView()
        {
            PortfolioListView.ItemsSource = AgentPortfolioStorage;
            PortfolioCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(PortfolioListView.ItemsSource);
        }
        private void SavePortfolioSettings()
        {
            List<AgentPortfolio> obj = AgentPortfolioStorage.Select(a => a).ToList();
            var fStream = new FileStream("PortfolioSettings.xml", FileMode.Create, FileAccess.Write, FileShare.None);
            var xmlSerializer = new XmlSerializer(typeof(List<AgentPortfolio>), new Type[] { typeof(AgentPortfolio) });
            xmlSerializer.Serialize(fStream, obj);
            fStream.Close();

        }
        private void InitiatePortfolioSettings()
        {
            StreamReader sr = new StreamReader("PortfolioSettings.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<AgentPortfolio>), new Type[] { typeof(AgentPortfolio) });
                var portfolios = (List<AgentPortfolio>)xmlSerializer.Deserialize(sr);
                sr.Close();
                if (portfolios == null) return;
                foreach (var rs in portfolios)
                {
                    AgentPortfolioStorage.Add(rs);
                }
                PortfolioListView.ItemsSource = AgentPortfolioStorage;
                IsPortfolioSettingsLoaded = true;
            }
            catch (Exception e)
            {
                IsPortfolioSettingsLoaded = false;
                sr.Close();
                Logger.Log(LogLevel.Error, e.Message);
                Logger.Log(LogLevel.Error, e.InnerException.Message);
                if (e.InnerException.Message == "Root element is missing.")
                    File.WriteAllText("PortfolioSettings.xml", string.Empty);
            }
        }
        private void AddAgentPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new AgentPortfolioAddition();
            form.ShowDialog();
            form = null;
            //Todo:save settings
        }
        private void EditPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = PortfolioListView.SelectedItems.Cast<AgentPortfolio>().ToList();
            //TODO:переделать
            foreach (var portfolioEditkWindow in from agentSettings in listToEdit
                                                 let index = AgentPortfolioStorage.IndexOf(agentSettings)
                                                 where index != -1
                                                 select new AgentPortfolioAddition(agentSettings, index))
            {
                portfolioEditkWindow.Title = "Аист Трейдер - Редактировать портфель";
                portfolioEditkWindow.ShowDialog();
                portfolioEditkWindow.Close();
            }
        }


        private void DelPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in PortfolioListView.SelectedItems.Cast<AgentPortfolio>().ToList())
            {
                AgentPortfolioStorage.Remove(item);
                SavePortfolioSettings();
            }
        }
        private void PortfolioListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsPortfolioSettingsLoaded && (File.Exists("PortfolioSettings.xml")))
                InitiatePortfolioSettings();
            //if (AgentPortfolioStorage.Count > 0)
            //    EditSingleOrGroupItemBtn.IsEnabled = true;
            //else
            //    EditSingleOrGroupItemBtn.IsEnabled = false;
        }
        private void AgentPortfolioStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
        }
    }
}
