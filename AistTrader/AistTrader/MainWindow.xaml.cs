using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AistTrader.Annotations;
using Common.Entities;
using Common.Params;
using Ecng.Collections;
using Ecng.Common;
using Ecng.Xaml;
using MahApps.Metro.Controls.Dialogs;
using MoreLinq;
using NLog;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    public partial class MainWindow: INotifyPropertyChanged
    {
        #region Fields

        public string DefaulConnectionName
        {
            get
            {
                return "TEst";
            }
            set { } 
        }
        public static MainWindow Instance { get; private set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private static readonly Logger TradesLogger = LogManager.GetLogger("TradesLogger");
        public ObservableCollection<Agent> AgentsStorage { get; private set; }
        public ObservableCollection<Connection> ConnectionsStorage { get; private set; }
        public ObservableCollection<Common.Entities.Portfolio> AgentPortfolioStorage { get; private set; }
        public ObservableCollection<AgentManager> AgentManagerStorage { get; private set; }

        public CollectionView AgentCollectionView { get; set; }
        public CollectionView ProviderCollectionView { get; set; }
        public CollectionView PortfolioCollectionView { get; set; }
        public CollectionView AgentManagerCollectionView { get; set; }
        private GridLength LogWindowPreviousHight;

        private readonly PortfoliosWindow _portfoliosWindow = new PortfoliosWindow();
        private readonly OrdersWindow _ordersWindow = new OrdersWindow();
        private readonly SecuritiesWindow _securitiesWindow = new SecuritiesWindow();
        private readonly MyTradesWindow _myTradesWindow = new MyTradesWindow();
        private readonly MonitorWindow _monitorWindow = new MonitorWindow();
        private string _defaultConnectionStatusBarText;
        
        public string DefaultConnectionStatusBarText
        {
            get { return _defaultConnectionStatusBarText; }
            set
            {
                _defaultConnectionStatusBarText = value;

                OnPropertyChanged(new PropertyChangedEventArgs("DefaultConnectionStatusBarText"));

            }
        }
        string[] EntitiesFilesNames = { "Agents.xml", "Portfolios.xml", "Connections.xml", "AgentManagerSettings.xml" };
        #endregion

        public MainWindow()
        {
            String name = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcesses().Count(p => p.ProcessName == name) > 1)
                Application.Current.Shutdown();
            //string buFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);



            //string buFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //string destFilePath = Path.Combine(buFilePath, "AistTrader");
            //if (!Directory.Exists(destFilePath)) Directory.CreateDirectory(new Uri(destFilePath).LocalPath);
            //foreach (var xmlset in EntitiesFilesNames)
            //{
            //    FileInfo copyToPath = new FileInfo(Path.Combine(System.Windows.Forms.Application.StartupPath, xmlset));
            //    FileInfo locaFilesInfo = new FileInfo(Path.Combine(destFilePath, copyToPath.FullName.Split('\\').Last()));
            //    if (!copyToPath.Exists)
            //    {
            //        if (locaFilesInfo.Exists)
            //        {
            //            locaFilesInfo.CopyTo(copyToPath.FullName);
            //        }
            //    }
            //}



            Instance = this;
            DataContext = this;
            ConnectionManager = new AistTraderConnnectionManager();
            AgentConnnectionManager = new AistTraderStrategiesConnnectionManager();
#region Initialize collections

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                this.TimeTextBlock.Text = String.Format("{0:G} (Local)", TimeHelper.Now);
            }, this.Dispatcher);
            
            AgentsStorage = new ObservableCollection<Agent>();
            AgentsStorage.CollectionChanged += AgentSettingsStorageChanged;

            ConnectionsStorage = new ObservableCollection<Connection>();
            ConnectionsStorage.CollectionChanged += ProviderStorageOnCollectionChanged;

            AgentPortfolioStorage = new ObservableCollection<Common.Entities.Portfolio>();
            AgentPortfolioStorage.CollectionChanged += AgentPortfolioStorageOnCollectionChanged; 

            AgentManagerStorage = new ObservableCollection<AgentManager>();
            AgentManagerStorage.CollectionChanged += AgentManagerStorage_CollectionChanged;

            _portfoliosWindow.MakeHideable();
            _ordersWindow.MakeHideable();
            _myTradesWindow.MakeHideable();
            _securitiesWindow.MakeHideable();
            _monitorWindow.MakeHideable();
            KeyUp += new KeyEventHandler(OKP); // подписываемся на события нажатия клавиш
            #endregion
        }
        public void OKP(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                if (AgentItem.IsSelected && DelAgentBtn.IsEnabled)
                {
                    DeleteAgentBtnClick(sender, e);
                }
                else if (AgentManagerItem.IsSelected && DelAgentManagerBtn.IsEnabled)
                {
                    DelAgentManagerBtnClick(sender, e);
                }
                else if (ProviderItem.IsSelected && DelAgentConnectionBtn.IsEnabled)
                {
                    DelAgentConnectionBtnClick(sender, e);
                }
                else if (PortfolioItem.IsSelected && DelPortfolioBtn.IsEnabled)
                {
                    DelPortfolioBtnClick(sender, e);
                }
            }
        }

        private void AgentManagerStorage_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void SetConnectionCommandStatus()
        {
            Instance.ConnectionsStorage.ForEach(i=>i.ConnectionParams.Command= OperationCommand.Connect);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.IsConnected = false);
            Instance.AgentManagerStorage.ForEach(i => i.AgentManagerSettings.Command =ManagerParams.AgentManagerOperationCommand.Start);
            Instance.AgentManagerStorage.ForEach(i => i.AgentManagerSettings.AgentMangerCurrentStatus = ManagerParams.AgentManagerStatus.Stopped);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.IsRegistredConnection = false);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.ConnectionState = ConnectionParams.ConnectionStatus.Disconnected);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.Accounts = new List<StockSharp.BusinessEntities.Portfolio>() );
            //Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.Tools = new List<Security>());
        }
        private void TabCtr_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is TabControl && AgentItem != null && AgentItem.IsSelected)
            {
            }
            if (e.OriginalSource is TabControl && ProviderItem != null && ProviderItem.IsSelected)
            {
            }
            if (e.OriginalSource is TabControl && PortfolioItem != null && PortfolioItem.IsSelected)
            {
            }
            if (e.OriginalSource is TabControl && AgentManagerItem != null && AgentManagerItem.IsSelected)
            {

                //todo: на все коннекты что есть проверка на активное состояние

                //var anyActive = Instance.ConnectionManager.Connections.Any(i=>i.ConnectionState == ConnectionStates.Connected);


                //if (!anyActive)
                //{
                //    AddAgentManagerBtn.ToolTip = "No active connections, cannot retrieve any securities";
                //    AddAgentManagerBtn.IsEnabled = false;
                //}
                //if (anyActive)
                //{
                //    AddAgentManagerBtn.IsEnabled = true;
                //}

            }
        }

        private void NtpMoexSync()
        {
            try
            {
                TimeHelper.SyncMarketTime(10000);
            }
            catch
            {
                this.TimeErrorTextBlock.Text = "Error moex time sync";
                //this.TimeErrorTextBlock.Foreground = new SolidColorBrush(Colors.White);
                TimeErrorTextBlock.FontWeight = FontWeights.Bold;
            }
        }

        private void AgentAddConfigMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = AgentItem;
        }
        private void AgentManagerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = AgentManagerItem;
        }
        private void ProviderManagerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = ProviderItem;
        }
        private void PortfolioMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = PortfolioItem;
        }
        private void WhatsNewItem_OnClick(object sender, RoutedEventArgs e)
        {
            var form = new WhatsNew().ShowDialog();
            form = null;
        }
        private void ShowPortfoliosClick(object sender, RoutedEventArgs e)
        {
            ShowOrHide(_portfoliosWindow);
        }
        private void ShowSecuritiesClick(object sender, RoutedEventArgs e)
        {
            ShowOrHide(_securitiesWindow);
        }
        private void ShowMyTradesClick(object sender, RoutedEventArgs e)
        {
            ShowOrHide(_myTradesWindow);
        }
        private void ShowOrdersClick(object sender, RoutedEventArgs e)
        {
            ShowOrHide(_ordersWindow);
        }
        private void ShowMonitorWindowClick(object sender, RoutedEventArgs e)
        {
            ShowOrHide(_monitorWindow);
        }
        private static void ShowOrHide(Window window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            if (window.Visibility == Visibility.Visible)
                window.Hide();
            else
                window.Show();
        }
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;
            //var mySettings = new MetroDialogSettings()
            //{
            //    AffirmativeButtonText = "Quit",
            //    NegativeButtonText = "Cancel",
            //    AnimateShow = true,
            //    AnimateHide = false
            //};
            //var result = await this.ShowMessageAsync("Quit application?",
            //    "Sure you want to quit application?",
            //    MessageDialogStyle.AffirmativeAndNegative, mySettings);
            //_shutdown = result == MessageDialogResult.Affirmative;

            //if (_shutdown)
            SaveProviderItems();
            BackUpXMLSettings();
            Application.Current.Shutdown();
        }

        private void BackUpXMLSettings()
        {
            string buFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string destFilePath = Path.Combine(buFilePath, "AistTrader");   
            if (!Directory.Exists(destFilePath)) Directory.CreateDirectory(new Uri(destFilePath).LocalPath);

            foreach (var xmlset in EntitiesFilesNames)
            {
                FileInfo sourceFilePath = new FileInfo(Path.Combine(System.Windows.Forms.Application.StartupPath, xmlset));
                FileInfo destFilePathWithFileName = new FileInfo(Path.Combine(destFilePath, sourceFilePath.FullName.Split('\\').Last()));
                if (destFilePathWithFileName.Exists)
                {
                    if (sourceFilePath.LastWriteTime > destFilePathWithFileName.LastWriteTime)
                        sourceFilePath.CopyTo(destFilePathWithFileName.FullName, true);
                }
                else
                {
                    if (sourceFilePath.Exists)
                        sourceFilePath.CopyTo(destFilePathWithFileName.FullName);
                }
            }
        }
        private void LaunchAppOnGitHub(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/igorshepe/-AistTrader");
        }
        private void ConnectionStatusTextBlock_OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (!Instance.ConnectionsStorage.IsEmpty())
            {
                var any = Instance.ConnectionsStorage.Any(i => i.ConnectionParams.IsDefaulConnection);
                if (any)
                {
                    var item =
                        Instance.ConnectionsStorage.FirstOrDefault(i => i.ConnectionParams.IsDefaulConnection) as
                            Connection;
                    ConnectionStatusTextBlock.ToolTip = string.Format("{0}" + " is set as default connection",
                        item.DisplayName);
                }
                else
                {
                    ConnectionStatusTextBlock.ToolTip = "Default connection is not set";
                    ConnectionStatusTextBlock.Text = "Disconnected";
                }

            }
            else
            {
                ConnectionStatusTextBlock.ToolTip = "Default connection is not set";
                ConnectionStatusTextBlock.Text = "Disconnected";
            }

        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs name, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
