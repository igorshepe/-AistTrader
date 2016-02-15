using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Common.Entities;
using Common.Settings;
using MoreLinq;
using NLog;
using StockSharp.BusinessEntities;

namespace AistTrader
{
    public partial class MainWindow
    {
        #region Fields
        public static MainWindow Instance { get; private set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<Agent> AgentsStorage { get; private set; }
        public ObservableCollection<AgentConnection> ProviderStorage { get; private set; }
        public ObservableCollection<AgentPortfolio> AgentPortfolioStorage { get; private set; }
        public ObservableCollection<AgentManager> AgentManagerStorage { get; private set; }

        public CollectionView AgentCollectionView { get; set; }
        public CollectionView ProviderCollectionView { get; set; }
        public CollectionView PortfolioCollectionView { get; set; }
        public CollectionView AgentManagerCollectionView { get; set; }
        
        #endregion
        public MainWindow()
        {
            Instance = this;
            ConnectionManager = new AistTraderConnnectionManager();
            #region Initialize collections
            AgentsStorage = new ObservableCollection<Agent>();
            AgentsStorage.CollectionChanged += AgentSettingsStorageChanged;

            ProviderStorage = new ObservableCollection<AgentConnection>();
            ProviderStorage.CollectionChanged += ProviderStorageOnCollectionChanged;

            AgentPortfolioStorage = new ObservableCollection<AgentPortfolio>();
            AgentPortfolioStorage.CollectionChanged += AgentPortfolioStorageOnCollectionChanged; 

            AgentManagerStorage = new ObservableCollection<AgentManager>();
            #endregion
        }

        private void SetConnectionCommandStatus()
        {
            Instance.ProviderStorage.ForEach(i=>i.Connection.Command= OperationCommand.Connect);
            Instance.ProviderStorage.ForEach(i => i.Connection.IsConnected = false);
            Instance.ProviderStorage.ForEach(i => i.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Disconnected);
            Instance.ProviderStorage.ForEach(i => i.Connection.Accounts = new List<Portfolio>() );
            Instance.ProviderStorage.ForEach(i => i.Connection.Tools = new List<Security>());
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
    }
}
