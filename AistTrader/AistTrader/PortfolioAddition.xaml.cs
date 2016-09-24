using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using StockSharp.Messages;
using Portfolio = Common.Entities.Portfolio;

namespace AistTrader
{
    public partial class PortfolioAddition : IDataErrorInfo
    {
        #region Fields

        private int EditIndex { get; set; }
        public ObservableCollection<Agent> AgentPortfolioStorage { get; private set; }
        private string _portfolioName;
        public string PortfolioName
        {
            get { return _portfolioName; }
            set { _portfolioName = value; }
        }
        private string _registeredProvider;
        public string RegisteredProvider
        {
            get { return _registeredProvider; }
            set { _registeredProvider = value; }
        }
        private string _selectedRegisteredProvider;
        public string SelectedRegisteredProvider
        {
            get { return _selectedRegisteredProvider; }
            set { _selectedRegisteredProvider = value; }
        }
        private string _selectedAccount;
        public string SelectedAccount
        {
            get { return _selectedAccount; }
            set { _selectedAccount = value; }
        }
        private bool _isEditMode;
        public List<Portfolio> DynamicAccount { get; set; }
        private Dictionary<string, bool> validPortflolioProperties = new Dictionary<string, bool>();
        #endregion
        public PortfolioAddition()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
            ConnectionProviderComboBox.Focus();
        }

        private void LoadParams()
        {
            var connections = MainWindow.Instance.ConnectionsStorage.Where(i => i.ConnectionParams.IsConnected).Select(i => i.DisplayName + " (" + i.ConnectionParams.Code + ")").ToList();
            ConnectionProviderComboBox.ItemsSource = connections;
            connections = null;
        }

        public PortfolioAddition(Portfolio portfolio, int editIndex)
        {
            _isEditMode = true;
            InitializeComponent();
            DataContext = this;
            EditIndex = editIndex;
            InitFields(portfolio);
        }

        private void InitFields(Portfolio portfolio)
        {
            var connNames= MainWindow.Instance.ConnectionManager.Connections.Where(i => i.ConnectionState == ConnectionStates.Connected).Select(i => i.ConnectionName).ToList();
            ConnectionProviderComboBox.ItemsSource = connNames;
            connNames = null;
            if (ConnectionProviderComboBox.Items.Count == 0)
            {
                ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ConnectionsStorage.Select(i => i.DisplayName).ToList();
            }
            var connections = MainWindow.Instance.ConnectionsStorage.Where(i => i.Id == portfolio.Connection.Id).Select(i => i.DisplayName).ToList();
            foreach (var i in connections)
            {
                _selectedRegisteredProvider = i.ToString();
            }
            connections = null;
            AccountComboBox.ItemsSource = portfolio.Connection.ConnectionParams.Accounts;
            _selectedAccount = portfolio.Code;
            _portfolioName = portfolio.Name;
            ConnectionProviderComboBox.IsEnabled = false;
            AccountComboBox.IsEnabled = false;
            ConnectionProviderComboBox.Focus();
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            var connectionProvider = ConnectionProviderComboBox.SelectedItem.ToString();
            if (EditIndex == int.MinValue)
            {
                connectionProvider = connectionProvider.Substring(0, connectionProvider.IndexOf(" (", StringComparison.Ordinal));
            }
            var selectedAccount = MainWindow.Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == connectionProvider)?.Portfolios.First();
            var agentItem = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.DisplayName == connectionProvider.ToString());
            agentItem.ConnectionParams.SelectedAccount = selectedAccount as StockSharp.BusinessEntities.Portfolio;
            connectionProvider = null;
            if (MainWindow.Instance.AddNewAgentPortfolio(new Common.Entities.Portfolio(PortfolioNameTxtBox.Text, agentItem, selectedAccount.Name, (StockSharp.BusinessEntities.Portfolio)selectedAccount), EditIndex))
            {
                Close();
            }
        }

        public string this[string columnName]
        {
            get
            {
                string validationResult = null;
                switch (columnName)
                {
                    case "PortfolioName":
                        validationResult = ValidatePortfolioName();
                        break;
                    case "RegisteredProvider":
                        validationResult = ValidateRegisteredProvider();
                        break;
                    case "SelectedAccount":
                        validationResult = ValidateDynamicAccount();
                        break;
                }
                string error = validationResult;
                validPortflolioProperties[columnName] = string.IsNullOrEmpty(error);
                if (validPortflolioProperties.Count == 3)
                {
                    OkPortfolioBtn.IsEnabled = validPortflolioProperties.Values.All(isValid => isValid);
                }
                return validationResult;
            }
        }

        private string ValidateRegisteredProvider()
        {
            if (string.IsNullOrEmpty(RegisteredProvider) & ConnectionProviderComboBox.Items.Count > 0) { return "Не выбран поставщик"; }
            if (ConnectionProviderComboBox.Items.Count == 0) { return "Нет активных подключений"; }
            return string.Empty;
        }
        
        private string ValidateDynamicAccount()
        {
            if (string.IsNullOrEmpty(SelectedAccount)) { return "Счёт не получен или все доступные счета уже задействованы"; }
            return string.Empty;
        }

        private string ValidatePortfolioName()
        {
            if (string.IsNullOrEmpty(PortfolioName)) { return "Задайте имя"; }
            if (PortfolioName.Length < 5) { return "Имя должно содержать не меньше 5 символов."; }
            return string.Empty;
        }

        public string Error { get; private set; }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ConnectionProviderComboBox.SelectedItem.ToString();
            if (item.Contains('('))
            {
                item = item.Substring(0, item.IndexOf(" (", StringComparison.Ordinal));
            }
            //Active
            var agent = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.DisplayName == item);
            item = null;
            if (agent.ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Connected && agent.ConnectionParams.Accounts.Count != 0)
            {
                if (_isEditMode)
                {
                    var accounts = agent.ConnectionParams.Accounts.Select(i => i.Name).ToList();
                    AccountComboBox.ItemsSource = accounts;
                    AccountComboBox.SelectedItem = _selectedAccount;
                    accounts = null;
                }
                else
                {
                    var accounts = agent.ConnectionParams.Accounts.Select(i => i.Name).ToList().Except(MainWindow.Instance.AgentPortfolioStorage.Select(i=>i.Code)).ToList();
                    AccountComboBox.ItemsSource = accounts;
                    accounts = null;
                }
            }
            //Not Active
            if (agent.ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Disconnected)
            {
                var accounts = MainWindow.Instance.AgentPortfolioStorage.Select(i => i.Code).ToList();
                AccountComboBox.ItemsSource = accounts;
                AccountComboBox.SelectedItem = _selectedAccount;
                accounts = null;
            }
        }
    }
}
