using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common.Entities;
using Ecng.Common;
using Ecng.ComponentModel;
using StockSharp.BusinessEntities;
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
        public List<Portfolio> DynamicAccount { get; set; }
        private Dictionary<string, bool> validPortflolioProperties = new Dictionary<string, bool>();

        #endregion

        //TODO: убрать лишнее, как будет проверяться динамика получаемая во время коннекта

        public PortfolioAddition()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }

        private void LoadParams()
        {
            ConnectionProviderComboBox.ItemsSource =
                MainWindow.Instance.ConnectionsStorage.Where(i => i.ConnectionParams.IsConnected)
                    .Select(i => i.DisplayName + " (" + i.ConnectionParams.Code + ")")
                    .ToList();
            //если 0 то ошибка- "нет активных подключений"
            //если > 0 то ошибка- 
            //#Oldie# ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ConnectionsStorage.Select(i => i.Name + " (" + i.Connection.Code + ")").ToList();
//            _dynamicAccount = MainWindow.Instance.PortfoliosList.Select(i => i.Name).ToList();
            //TODO:Загрузка счёта
        }

        public PortfolioAddition(Portfolio portfolio, int editIndex)
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = editIndex;
            InitFields(portfolio);
        }

        private void InitFields(Portfolio portfolio)
        {
            //выбирать либо напрямую с менеджера подключений либо/ибо из айтема, где предавариельно ставим, что данный объект активен
            ConnectionProviderComboBox.ItemsSource =MainWindow.Instance.ConnectionManager.Connections.Where(i => i.ConnectionState == ConnectionStates.Connected).Select(i => i.ConnectionName).ToList();
            if (ConnectionProviderComboBox.Items.Count ==0)
                ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ConnectionsStorage.Select(i => i.DisplayName).ToList();    

            var items = ConnectionProviderComboBox.ItemsSource;
            //var index= MainWindow.Instance.ConnectionsStorage.ToList().FindIndex(i => i.Name == portfolio.Connection.Name);
            foreach (var i in items)
            {
                if (i.ToString() == portfolio.Connection.DisplayName)
                {
                    var selectedProvider = i;
                    _selectedRegisteredProvider = selectedProvider.ToString();
                    break;
                }
            }
            //ConnectionProviderComboBox.SelectedItem= items;
            //int index = MainWindow.Instance.ConnectionsStorage.Where<AgentConnection>(x => x.Name == portfolio.Connection.Name).Select<AgentConnection, int>(x => MainWindow.Instance.ConnectionsStorage.IndexOf(x)).Single<int>();
            //portfolio.Connection.Name;
            //Todo: переделать под динамику
            AccountComboBox.ItemsSource = portfolio.Connection.ConnectionParams.Accounts;
             _selectedAccount = portfolio.Code;
            _portfolioName = portfolio.Name;
            ConnectionProviderComboBox.IsEnabled = false;
            AccountComboBox.IsEnabled = false;

            //DynamicAccount = new List<string> {"Allem", "Vinny"};
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountComboBox.SelectedItem;
            var connectionProvider = ConnectionProviderComboBox.SelectedItem.ToString();
            if (EditIndex == int.MinValue)
                connectionProvider = connectionProvider.Substring(0,
                    connectionProvider.IndexOf(" (", StringComparison.Ordinal));

            var agentItem =
                MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.DisplayName == connectionProvider.ToString());
            agentItem.ConnectionParams.SelectedAccount = selectedAccount as StockSharp.BusinessEntities.Portfolio;
            MainWindow.Instance.AddNewAgentPortfolio(new Common.Entities.Portfolio(PortfolioNameTxtBox.Text, agentItem, selectedAccount.ToString()), EditIndex);
            Close();
        }

        //private static AgentConnection GetEditedItem(int index)
        //{
        //    if (index < 0 || index >= ProviderManagerForm.Instance.AgentAccountStorage.Count) return null;

        //    return ProviderManagerForm.Instance.AgentAccountStorage[index];
        //}
        private void AccountComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ConnectionProviderComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditIndex != int.MinValue)
            {
                var connectionProvider = ConnectionProviderComboBox.SelectedItem.ToString();
                var agent = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.DisplayName == connectionProvider);
                if (agent.ConnectionParams.Accounts != null) AccountComboBox.ItemsSource = agent.ConnectionParams.Accounts.ToList();
                //AccountComboBox.SelectedItem = AgentPortfolioStorage.Select()
            }
            else
            {
                var connectionProvider = ConnectionProviderComboBox.SelectedItem.ToString();
                connectionProvider = connectionProvider.Substring(0,
                    connectionProvider.IndexOf(" (", StringComparison.Ordinal));
                var agent = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.DisplayName == connectionProvider);
                //if (agent.Connection.Accounts != null) 
                //AccountComboBox.ItemsSource = agent.Connection.Accounts.ToList();
                //TODO: уточнить что делать если данных нет    
            }
        }
        public string this[string columnName]
        {
            //имя уникальное всегда
            //ограничений на использование поставщика нет, можно много портфелей на одного поставщика подключать
            // если мы уже используем в портфеле динамический счёт, который был получен и задействован- мы его не отображаем вообще
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
                    case "DynamicAccount":
                        validationResult = ValidateDynamicAccount();
                        break;
                    //case "ProviderPath":
                    //    validationResult = ValidateProviderPath();
                    //    break;
                    //default:
                    //    throw new ApplicationException("Unknown Property being validated on Product.");
                }
                string error = validationResult;
                validPortflolioProperties[columnName] = String.IsNullOrEmpty(error) ? true : false;
                if (validPortflolioProperties.Count == 2)
                    OkPortfolioBtn.IsEnabled = validPortflolioProperties.Values.All(isValid => isValid);
                return validationResult;
            }
        }

        private string ValidateRegisteredProvider()
        {
            if (String.IsNullOrEmpty(this.RegisteredProvider) & ConnectionProviderComboBox.Items.Count > 0)
            {
                return "Не выбран поставщик";
            }
            if (ConnectionProviderComboBox.Items.Count == 0)
            {
                return "Нет активных подключений";
            }
            return String.Empty;
        }

        private string ValidateDynamicAccount()
        {
            //if (this.DynamicAccount.Count==0)//TODO: вот тут подтянуть указатель на соединение которое используется для получения данного счёта
            //    return "Счёт не получен, соединение не активно";
            //if (AccountComboBox.SelectedItem==null)
            //    return "Не выбран счёт";
            return String.Empty;
        }

        private string ValidatePortfolioName()
        {
            if (String.IsNullOrEmpty(this.PortfolioName))
                return "Задайте имя";
            else if (this.PortfolioName.Length < 5)
                return "Имя должно содержать не меньше 5 символов.";
            //else if (NameAlredyInUse)
            //    return "Данное имя уже используется";
            return String.Empty;
        }

        public string Error { get; private set; }

        //private void AccountComboBox_OnLoaded(object sender, RoutedEventArgs e)
        //{

        //    AccountComboBox.ItemsSource = MainWindow.Instance.PortfoliosList.Select(i => i.Name).ToList();
        //}
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ConnectionProviderComboBox.SelectedItem.ToString();
            if (item.Contains('('))
                item = item.Substring(0, item.IndexOf(" (", StringComparison.Ordinal));
            var agent = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.DisplayName == item);
            var accounts = agent.ConnectionParams.Accounts.Select(i=>i.Name).ToList();
            AccountComboBox.ItemsSource = accounts;
            AccountComboBox.SelectedItem = _selectedAccount;
        }
    }
}
