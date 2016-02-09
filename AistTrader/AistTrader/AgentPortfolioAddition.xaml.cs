using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common.Entities;
using StockSharp.BusinessEntities;
using StockSharp.Messages;

namespace AistTrader
{
    public partial class AgentPortfolioAddition : IDataErrorInfo
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

        public List<Portfolio> DynamicAccount { get; set; }
        private Dictionary<string, bool> validPortflolioProperties = new Dictionary<string, bool>();

        #endregion

        //TODO: убрать лишнее, как будет проверяться динамика получаемая во время коннекта

        public AgentPortfolioAddition()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }

        private void LoadParams()
        {
            ConnectionProviderComboBox.ItemsSource =
                MainWindow.Instance.ProviderStorage.Where(i => i.Connection.IsConnected)
                    .Select(i => i.Name + " (" + i.Connection.Code + ")")
                    .ToList();
            //если 0 то ошибка- "нет активных подключений"
            //если > 0 то ошибка- 
            //#Oldie# ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ProviderStorage.Select(i => i.Name + " (" + i.Connection.Code + ")").ToList();
//            _dynamicAccount = MainWindow.Instance.PortfoliosList.Select(i => i.Name).ToList();
            //TODO:Загрузка счёта
        }

        public AgentPortfolioAddition(AgentPortfolio portfolio, int editIndex)
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = editIndex;
            InitFields(portfolio);
        }

        private void InitFields(AgentPortfolio portfolio)
        {
            //выбирать либо напрямую с менеджера подключений либо/ибо из айтема, где предавариельно ставим, что данный объект активен
            ConnectionProviderComboBox.ItemsSource =
                MainWindow.Instance.ConnectionManager.Connections.Where(
                    i => i.ConnectionState == ConnectionStates.Connected).Select(i => i.Name).ToList();
            //или
            //ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ProviderStorage.Where(i=>i.Connection.).Select(i => i.Name).ToList();



            var items = ConnectionProviderComboBox.ItemsSource;
            //var index= MainWindow.Instance.ProviderStorage.ToList().FindIndex(i => i.Name == portfolio.Connection.Name);
            foreach (var i in items)
            {
                if (i.ToString() == portfolio.Connection.Name)
                {
                    var selectedProvider = i;
                    _selectedRegisteredProvider = selectedProvider.ToString();
                    break;
                }
            }
            //ConnectionProviderComboBox.SelectedItem= items;
            //int index = MainWindow.Instance.ProviderStorage.Where<AgentConnection>(x => x.Name == portfolio.Connection.Name).Select<AgentConnection, int>(x => MainWindow.Instance.ProviderStorage.IndexOf(x)).Single<int>();
            //portfolio.Connection.Name;
            //Todo: переделать под динамику
            //AccountComboBox.ItemsSource = portfolio.Connection.Connection.Accounts;
            _portfolioName = portfolio.Name;
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
                MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == connectionProvider.ToString());
            agentItem.Connection.SelectedAccount = selectedAccount as Portfolio;
            MainWindow.Instance.AddNewAgentPortfolio(new AgentPortfolio(PortfolioNameTxtBox.Text, agentItem), EditIndex);
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
                var agent = MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == connectionProvider);
                if (agent.Connection.Accounts != null) AccountComboBox.ItemsSource = agent.Connection.Accounts.ToList();
                //AccountComboBox.SelectedItem = AgentPortfolioStorage.Select()
            }
            else
            {
                var connectionProvider = ConnectionProviderComboBox.SelectedItem.ToString();
                connectionProvider = connectionProvider.Substring(0,
                    connectionProvider.IndexOf(" (", StringComparison.Ordinal));
                var agent = MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == connectionProvider);
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
            //throw new NotImplementedException();

            var item = ConnectionProviderComboBox.SelectedItem.ToString();
            item = item.Substring(0, item.IndexOf(" (", StringComparison.Ordinal));
            var agent = MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == item);
            var accounts = agent.Connection.Accounts;
            List<Portfolio> portfolios = new List<Portfolio>();
            foreach (var i in accounts)
            {
                portfolios.Add(i);
            }
            AccountComboBox.ItemsSource = portfolios.ToList();

        }
    }
}
