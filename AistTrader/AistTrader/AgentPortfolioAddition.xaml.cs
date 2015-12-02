using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common.Entities;
using StockSharp.BusinessEntities;

namespace AistTrader
{
    public partial class AgentPortfolioAddition : IDataErrorInfo
    {
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
        private string _dynamicAccount;
        public string DynamicAccount
        {
            get { return _dynamicAccount; }
            set { _dynamicAccount = value; }

        }
        private Dictionary<string, bool> validPortflolioProperties = new Dictionary<string, bool>();

        public AgentPortfolioAddition()
        {
            
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }
        private void LoadParams()
        {
            ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ProviderStorage.Select(i => i.Name + " (" + i.Connection.Code + ")").ToList();
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
            ConnectionProviderComboBox.ItemsSource = MainWindow.Instance.ProviderStorage.Select(i => i.Name).ToList();
            ConnectionProviderComboBox.SelectedItem = portfolio.Connection.Name;
            AccountComboBox.ItemsSource = portfolio.Connection.Connection.Accounts;
            //AccountComboBox.SelectedItem = 
            PortfolioNameTxtBox.Text = portfolio.Name;
        }
        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            if (ConnectionProviderComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, @"Не выбран поставщик.");
                return;
            }
            if (AccountComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, @"Не выбран счёт.");
                return;
            }

            if (PortfolioNameTxtBox.Text.Length <= 0)
            {
                MessageBox.Show(this, @"Не задано имя портфеля.");
                return;
            }
            //var connection = new ConnectionSettings(connectionType, LoginTxtBox.Text, PasswordTxtBox.Password, QuikPath.Text, Trans2QuikName.Text, PathToRouter.Text);
            //var agentPortfolio = new AgentAccountSettings(ClienNameTxtBox.Text, ClienTxtBox.Text, LoginTxtBox.Text, PasswordTxtBox.Password, connection, false);
            var selectedAccount = AccountComboBox.SelectedItem;

            var connectionProvider = ConnectionProviderComboBox.SelectedItem.ToString();
            if (EditIndex == int.MinValue)
                connectionProvider = connectionProvider.Substring(0, connectionProvider.IndexOf(" (", StringComparison.Ordinal));

            var agentItem = MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == connectionProvider.ToString());
            agentItem.Connection.SelectedAccount = (Portfolio)selectedAccount;
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
                connectionProvider = connectionProvider.Substring(0, connectionProvider.IndexOf(" (", StringComparison.Ordinal));
                var agent = MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == connectionProvider);
                if (agent.Connection.Accounts != null) AccountComboBox.ItemsSource = agent.Connection.Accounts.ToList();
                //TODO: уточнить что делать если данных нет    
            }
        }




        public string this[string columnName]
        {

            //имя уникальное всегда

            //ограничений на использование поставщика нет, можно много портфелей на одного поставщика подключать

            // если мы уже используем в фортфеле динамический счёт, который был получен и задействован- мы его не отображаем вообще






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
                    default:
                        throw new ApplicationException("Unknown Property being validated on Product.");
                }
                string error = validationResult;
                validPortflolioProperties[columnName] = String.IsNullOrEmpty(error) ? true : false;
                if (validPortflolioProperties.Count == 3)
                    OkPortfolioBtn.IsEnabled = validPortflolioProperties.Values.All(isValid => isValid);
                return validationResult;
            }
        }



        
        private string ValidateRegisteredProvider()
        {
            if (String.IsNullOrEmpty(this.RegisteredProvider))
                return "Не выбран поставщик";
            return String.Empty;
        }
        private string ValidateDynamicAccount()
        {
            if (String.IsNullOrEmpty(this.RegisteredProvider))//TODO: вот тут подтянуть указатель на соединение которое используется для получения данного счёта
                return "Счёт не получен, соединение не активно";
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

    }
}
