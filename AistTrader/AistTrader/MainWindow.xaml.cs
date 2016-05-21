using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Common.Entities;
using Common.Params;
using Ecng.Collections;
using Ecng.Common;
using MahApps.Metro.Controls.Dialogs;
using MoreLinq;
using NLog;
using StockSharp.BusinessEntities;

namespace AistTrader
{
    public partial class MainWindow
    {
        #region Fields
        private bool _shutdown;

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
        private  readonly Logger LogView;
        public ObservableCollection<Agent> AgentsStorage { get; private set; }
        public ObservableCollection<Connection> ConnectionsStorage { get; private set; }
        public ObservableCollection<Common.Entities.Portfolio> AgentPortfolioStorage { get; private set; }
        public ObservableCollection<AgentManager> AgentManagerStorage { get; private set; }

        public CollectionView AgentCollectionView { get; set; }
        public CollectionView ProviderCollectionView { get; set; }
        public CollectionView PortfolioCollectionView { get; set; }
        public CollectionView AgentManagerCollectionView { get; set; }
        private GridLength LogWindowPreviousHight;
        #endregion

        public MainWindow()
        {
            Instance = this;
            ConnectionManager = new AistTraderConnnectionManager();
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


            #endregion
        }
        private void SetConnectionCommandStatus()
        {
            Instance.ConnectionsStorage.ForEach(i=>i.ConnectionParams.Command= OperationCommand.Connect);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.IsConnected = false);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.IsRegistredConnection = false);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.ConnectionState = ConnectionParams.ConnectionStatus.Disconnected);
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.Accounts = new List<StockSharp.BusinessEntities.Portfolio>() );
            Instance.ConnectionsStorage.ForEach(i => i.ConnectionParams.Tools = new List<Security>());
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

        private async void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };
            var result = await this.ShowMessageAsync("Quit application?",
                "Sure you want to quit application?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);
            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
                Application.Current.Shutdown();
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
    }
}
