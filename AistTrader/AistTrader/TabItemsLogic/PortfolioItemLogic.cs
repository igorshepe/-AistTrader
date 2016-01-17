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
            LoadPortfolioSettings();
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

            //var sortedList = AgentPortfolioStorage.OrderBy(set => "{0}-{1}".Put(set.Connection.Name, set.Connection.ToString())).ToList();
            //Settings.Default.AgentPortfolio = new SettingsArrayList(sortedList);
            //Settings.Default.Save();
        }
        private void LoadPortfolioSettings()
        {

            var xmlSerializer = new XmlSerializer(typeof(List<AgentPortfolio>), new Type[] { typeof(AgentPortfolio) });
            StreamReader sr = new StreamReader("PortfolioSettings.xml");
            var portfolios = (List<AgentPortfolio>)xmlSerializer.Deserialize(sr);
            sr.Close();
            if (portfolios == null) return;
            try
            {
                foreach (var rs in portfolios)
                {
                    AgentPortfolioStorage.Add(rs);
                }
                PortfolioListView.ItemsSource = AgentPortfolioStorage;
            }


            //if (Settings.Default.AgentPortfolio == null) return;
            //try
            //{
            //    foreach (var rs in Settings.Default.AgentPortfolio.Cast<AgentPortfolio>())
            //    {
            //        AgentPortfolioStorage.Add(rs);
            //    }
            //    PortfolioListView.ItemsSource = AgentPortfolioStorage;
            //}
            catch (Exception)
            {
                MessageBox.Show(this, @"Не удалось прочитать настройки. Задайте заново.");
                Settings.Default.AgentPortfolio.Clear();
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
