using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Common.Entities;
using NLog;

namespace AistTrader

    //todo: идентификаторы портфеля тоже в кеше

{
    public partial class MainWindow
    {
        public bool IsPortfolioSettingsLoaded;
        public void AddNewAgentPortfolio(Common.Entities.Portfolio settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentPortfolioStorage.Count)
                AgentPortfolioStorage[editIndex] = settings;
            else
                AgentPortfolioStorage.Add(settings);
            SavePortfolioSettings();

            UpdatePortfolioListView();
        }

        public void UpdatePortfolioListView()
        {
            PortfolioListView.ItemsSource = AgentPortfolioStorage;
            PortfolioCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(PortfolioListView.ItemsSource);
        }
        private void SavePortfolioSettings()
        {
            List<Common.Entities.Portfolio> obj = AgentPortfolioStorage.Select(a => a).ToList();
            var fStream = new FileStream("Portfolios.xml", FileMode.Create, FileAccess.Write, FileShare.None);
            var xmlSerializer = new XmlSerializer(typeof(List<Common.Entities.Portfolio>), new Type[] { typeof(Common.Entities.Portfolio) });
            xmlSerializer.Serialize(fStream, obj);
            fStream.Close();
        }
        private void InitiatePortfolioSettings()
        {
            SetConnectionCommandStatus();
            StreamReader sr = new StreamReader("Portfolios.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<Common.Entities.Portfolio>), new Type[] { typeof(Common.Entities.Portfolio) });
                var portfolios = (List<Common.Entities.Portfolio>)xmlSerializer.Deserialize(sr);
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
                    File.WriteAllText("Portfolios.xml", string.Empty);
            }
        }
        private void AddAgentPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new PortfolioAddition();
            form.ShowDialog();
            form = null;
            //Todo:save settings
        }
        private void EditPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = PortfolioListView.SelectedItems.Cast<Common.Entities.Portfolio>().ToList();
            //TODO:переделать
            foreach (var portfolioEditkWindow in from agentSettings in listToEdit
                                                 let index = AgentPortfolioStorage.IndexOf(agentSettings)
                                                 where index != -1
                                                 select new PortfolioAddition(agentSettings, index))
            {
                portfolioEditkWindow.Title = "Аист Трейдер - Редактировать портфель";
                portfolioEditkWindow.ShowDialog();
                portfolioEditkWindow.Close();
            }
        }
        private void DelPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in PortfolioListView.SelectedItems.Cast<Common.Entities.Portfolio>().ToList())
            {
                AgentPortfolioStorage.Remove(item);
                SavePortfolioSettings();
            }
        }
        private void PortfolioListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsPortfolioSettingsLoaded & (File.Exists("Portfolios.xml")) & AgentPortfolioStorage.Count == 0)
                InitiatePortfolioSettings();
            //if (AgentPortfolioStorage.Count > 0)
            //    EditSingleOrGroupItemBtn.IsEnabled = true;
            //else
            //    EditSingleOrGroupItemBtn.IsEnabled = false;
        }
        private void AgentPortfolioStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
        }
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mainGrid.RowDefinitions[2].Height.Value == 0)
            {
                mainGrid.RowDefinitions[2].Height = new GridLength(180);
                MainFrm.ImageInStackPanelOfStatusBar.Source = new BitmapImage(new Uri("Resources/Images/hide.png", UriKind.Relative));
            }
            else
            {
                mainGrid.RowDefinitions[2].Height = new GridLength(0);
                MainFrm.ImageInStackPanelOfStatusBar.Source = new BitmapImage(new Uri("Resources/Images/show.png", UriKind.Relative));
            }
        }
    }
}
    