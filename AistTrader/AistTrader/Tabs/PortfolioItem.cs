using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Common.Entities;
using Ecng.Common;
using NLog;

//todo: идентификаторы портфеля тоже в кеше
namespace AistTrader
{
    public partial class MainWindow
    {
        public bool IsPortfolioSettingsLoaded;

        public bool AddNewAgentPortfolio(Common.Entities.Portfolio settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < AgentPortfolioStorage.Count)
            {
                AgentPortfolioStorage.Clear();
                InitiatePortfolioSettings();
                AgentPortfolioStorage[editIndex] = settings;
                currentAgentManager.AgentManagerSettings.Portfolio = settings;
            }
            else
            {
                if (!AgentPortfolioStorage.Any(p => p.Name == settings.Name))
                {
                    AgentPortfolioStorage.Add(settings);
                }
                else
                {
                    MessageBox.Show("Portfolio with such name already exists. Choose another name.");
                    return false;
                }
            }
            SavePortfolioSettings();
            UpdatePortfolioListView();

            return true;
        }

        public void UpdatePortfolioListView()
        {
            PortfolioListView.ItemsSource = AgentPortfolioStorage;
            PortfolioCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(PortfolioListView.ItemsSource);
        }

        private void SavePortfolioSettings()
        {
            try
            {
                List<Portfolio> obj = AgentPortfolioStorage.Select(a => a).ToList();
                var tList = new List<Type>();
                tList.Add(typeof(Common.Entities.Portfolio));
                tList.Add(typeof(System.TimeZoneInfo));
                tList.Add(typeof(TimeZoneInfo.AdjustmentRule[]));
                tList.Add(typeof(TimeZoneInfo.AdjustmentRule));
                tList.Add(typeof(TimeZoneInfo.TransitionTime));
                tList.Add(typeof(System.DayOfWeek));
                using (var fStream = new FileStream("Portfolios.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(List<Portfolio>), tList);
                    xmlSerializer.WriteObject(fStream, obj);
                    fStream.Close();
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => Logger.Log(LogLevel.Error, ex.Message));
            }
        }

        private void InitiatePortfolioSettings()
        {
            using (FileStream fs = new FileStream("Portfolios.xml", FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var tList = new List<Type>();
                    tList.Add(typeof(Portfolio));
                    tList.Add(typeof(TimeZoneInfo));
                    tList.Add(typeof(TimeZoneInfo.AdjustmentRule[]));
                    tList.Add(typeof(TimeZoneInfo.AdjustmentRule));
                    tList.Add(typeof(TimeZoneInfo.TransitionTime));
                    tList.Add(typeof(DayOfWeek));
                    var xmlSerializer = new DataContractSerializer(typeof(List<Portfolio>), tList);
                    var portfolios = (List<Portfolio>)xmlSerializer.ReadObject(fs);
                    fs.Close();
                    if (portfolios == null) { return; }
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
                    fs.Close();
                    Task.Run(() => Logger.Log(LogLevel.Error, e.Message));
                    Task.Run(() => Logger.Log(LogLevel.Error, e.InnerException.Message));
                    if (e.InnerException.Message == "Root element is missing.")
                    {
                        File.WriteAllText("Portfolios.xml", string.Empty);
                    }
                }
            }
        }

        private void AddAgentPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new PortfolioAddition();
            form.ShowDialog();
            form = null;
            //Todo: save settings
        }

        private void EditPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = PortfolioListView.SelectedItems.Cast<Portfolio>().ToList();
            //TODO:переделать
            foreach (var portfolioEditkWindow in from agentSettings in listToEdit
                                                 let index = AgentPortfolioStorage.IndexOf(agentSettings)
                                                 where index != -1
                                                 select new PortfolioAddition(agentSettings, index))
            {
                portfolioEditkWindow.Title = "Aist Trader - Edit Portfolio";
                portfolioEditkWindow.ShowDialog();
                portfolioEditkWindow.Close();
            }
        }

        private void DelPortfolioBtnClick(object sender, RoutedEventArgs e)
        {
            var selectedPortfolio = PortfolioListView.SelectedItem as Portfolio;
            if (AgentManagerListView.Items.Cast<AgentManager>().Any(i =>
            {
                return selectedPortfolio != null && i.AgentManagerSettings.Portfolio.Name == selectedPortfolio.Name;
            }))
            {
                MessageBox.Show(this, @"Used in agent manager, can not be deleted!");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Portfolio \"{0}\" will be deleted! You sure?".Put(PortfolioListView.SelectedItem), "Delete connection", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in PortfolioListView.SelectedItems.Cast<Portfolio>().ToList())
                {
                    AgentPortfolioStorage.Remove(item);
                    SavePortfolioSettings();
                }
            }
        }

        private void PortfolioListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsPortfolioSettingsLoaded & (File.Exists("Portfolios.xml")) & AgentPortfolioStorage.Count == 0)
            {
                InitiatePortfolioSettings();
            }
            if (PortfolioListView.Items.Count == 0)
            {
                DelPortfolioBtn.IsEnabled = false;
                EditPortfolioBtn.IsEnabled = false;
            }
            else
            {
                DelPortfolioBtn.IsEnabled = true;
                EditPortfolioBtn.IsEnabled = true;
            }
        }
        private void AgentPortfolioStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
        }
        private void StasusBarLogImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mainGrid.RowDefinitions[2].Height.Value == 0)
            {
                mainGrid.RowDefinitions[2].Height = new GridLength(LogWindowPreviousHight.Value);
                MainFrm.ImageInStackPanelOfStatusBar.Source = new BitmapImage(new Uri("Resources/Images/hide.png", UriKind.Relative));
            }
            else
            {
                LogWindowPreviousHight = mainGrid.RowDefinitions[2].Height;
                mainGrid.RowDefinitions[2].Height = new GridLength(0);
                MainFrm.ImageInStackPanelOfStatusBar.Source = new BitmapImage(new Uri("Resources/Images/show.png", UriKind.Relative));
            }
        }

        private void PortfolioListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PortfolioListView.Items.Count == 0)
            {
                DelPortfolioBtn.IsEnabled = false;
                EditPortfolioBtn.IsEnabled = false;
            }
            else
            {
                DelPortfolioBtn.IsEnabled = true;
                EditPortfolioBtn.IsEnabled = true;
            }
        }
    }
}
    