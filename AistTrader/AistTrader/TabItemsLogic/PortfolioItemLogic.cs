using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

namespace AistTrader
{
    public partial class MainWindow
    {
        public ObservableCollection<AgentPortfolio> AgentPortfolioStorage { get; private set; }

        private void LoadPortfolioTabItemData()
        {
            AgentPortfolioStorage = new ObservableCollection<AgentPortfolio>();

            if (File.Exists("PortfolioSettings.xml"))
                InitiatePortfolioSettings();
        }

        public void AddNewAgentPortfolio(AgentPortfolio settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentPortfolioStorage.Count)
                AgentPortfolioStorage[editIndex] = settings;
            else
                AgentPortfolioStorage.Add(settings);
            SavePortfolioSettings();
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
            }
            catch (Exception e)
            {
                sr.Close();
                if (e.InnerException.Message == "Root element is missing.")
                    System.IO.File.WriteAllText("PortfolioSettings.xml", string.Empty);
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
    }
}
