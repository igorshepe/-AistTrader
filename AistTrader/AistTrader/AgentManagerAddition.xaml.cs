using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    public partial class AgentManagerAddition:IDataErrorInfo
    {
        private Dictionary<string, bool> validManagerProperties = new Dictionary<string, bool>();

        private string _selectedPortfolio;
        public string SelectedPortfolio
        {
            get { return _selectedPortfolio; }
            set { _selectedPortfolio = value; }

        }
        private string _portfolio; 
        public string Portfolio 
        {
            get { return _portfolio; }
            set { _portfolio = value; } 

        }
        private string _selectedGroupOrSingleAgent;
        public string SelectedGroupOrSingleAgent
        {
            get { return _selectedGroupOrSingleAgent; }
            set { _selectedGroupOrSingleAgent = value; }

        }
        private string _groupOrSingleAgent;
        public string GroupOrSingleAgent
        {
            get { return _groupOrSingleAgent; }
            set { _groupOrSingleAgent = value; }

        }
        private string _tools;
        public string Tools
        {
            get { return _tools; }
            set { _tools = value; } 

        }
        private int _amount;
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }

        }

        private int EditIndex { get; set; }
        public AgentManagerAddition()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
            var x = SecurityPicker;
            //TODO:указывать источник- подключение, для загрузки параметров
            x.SecurityProvider = new FilterableSecurityProvider(/*MainWindow.Instance.Trader*/);
            
            //workin'
            //SecurityPicker.SecurityProvider.Securities.AddRange(MainWindow.Instance.SecuritiesList);

        }
        public AgentManagerAddition(AgentManager agent, int editIndex)
        {
            InitializeComponent();
            DataContext = this;
            InitFields(agent);
            EditIndex = editIndex;
        }
        private void LoadParams()
        {
            List<string> resultsList = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName).ToList();
            var results = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName != "ungrouped agents").Select(i => i.Params.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList;

            //var agents = Settings.Default.AgentManager.Cast<AgentManager>().Select(i => i.Name).ToList();
            //var accounts = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().Select(i =>i).Except(agents).ToList();
            var accounts = MainWindow.Instance.AgentPortfolioStorage.Cast<AgentPortfolio>().Select(i => i.Name).ToList();
            PortfolioComboBox.ItemsSource = accounts;
            AmountTextBox.Text = "";

        }
        private void InitFields(AgentManager agent)
        {
            PortfolioComboBox.ItemsSource = MainWindow.Instance.AgentPortfolioStorage.Cast<AgentPortfolio>().Select(i => i.Name).ToList();
            _selectedPortfolio = agent.Name;

            List<string> resultsList = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Name).ToList();
            var results = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName != "ungrouped agents").Select(i => i.Params.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList;
            _selectedGroupOrSingleAgent = agent.AgentManagerSettings.AgentOrGroup;



            SecurityPicker.SelectedSecurity = agent.Tool;
            AmountTextBox.Text = agent.Amount.ToString();
        }
        private void AddConfigBtnClick(object sender, RoutedEventArgs e)
        {
            //if (AccountComboBox.SelectedIndex == -1)
            //{
            //    MessageBox.Show(this, @"Не выбран cчёт");
            //    return;
            //} 
            //if (GroupOrSingleAgentComboBox.SelectedIndex == -1)
            //{
            //    MessageBox.Show(this, @"Не выбран агент или группа агентов");
            //    return;
            //}
            //if (AmountTextBox.Visibility == Visibility.Visible && AmountTextBox.Text == "")
            //{
            //    MessageBox.Show(this, @"Не заполнен объем");
            //    return;
            //}
            //if (ToolComboBox.SelectedIndex == -1)
            //{
            //    MessageBox.Show(this, @"Не выбран инструмент");
            //    return;
            //}
            var s =  SecurityPicker.SelectedSecurity;
            AgentManagerSettings setting;
            var agentPortfolio = MainWindow.Instance.AgentPortfolioStorage.Cast<AgentPortfolio>().FirstOrDefault(i => i.Name == PortfolioComboBox.SelectedItem.ToString());
            var agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().FirstOrDefault(i => i.Params.FriendlyName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
            if (agent == null)
            {
                agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().FirstOrDefault(i => i.Params.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
                setting = new AgentManagerSettings(agentPortfolio, agent.Params.GroupName, SecurityPicker.SelectedSecurity);
            }
            else
                setting = new AgentManagerSettings(agentPortfolio, agent.Params.FriendlyName, SecurityPicker.SelectedSecurity);
            MainWindow.Instance.AddNewAgentManager(new AgentManager(setting.Account.Name , setting, setting.Tool,/* AmountTextBox.Value*/ 10), EditIndex);
            Close();
        }


        private void GroupOrSingleAgentComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var result = MainWindow.Instance.AgentsStorage.Cast<Agent>().Any
                (i => i.Params.GroupName != "ungrouped agents" && i.Params.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
            if (result)
            {
                AmountTextBox.Visibility = Visibility.Collapsed;
                AmountLbl.Visibility = Visibility.Collapsed;
            }
            else
            {
                AmountTextBox.Visibility = Visibility.Visible;
                AmountLbl.Visibility = Visibility.Visible;
            }
        }

        private void AccountComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPortfolio = MainWindow.Instance.AgentPortfolioStorage.Cast<AgentPortfolio>().FirstOrDefault(i => i.Name == (string)PortfolioComboBox.SelectedItem);

            ////имя счета
            //var item = AccountComboBox.SelectedItem.ToString();

            //var portfolio = MainWindow.Instance.AgentPortfolioStorage.FirstOrDefault(i => i.Name == item);
            //item = item.Substring(0, item.IndexOf(" (", StringComparison.Ordinal));
            //var agent = MainWindow.Instance.ProviderStorage.FirstOrDefault(i => i.Name == item);
            //var securities = selectedPortfolio.Connection.Connection.Tools;
            //List<Security> portfoliosList = new List<Security>();
            //foreach (var i in securities)
            //{
            //    portfoliosList.Add(i);
            //}

            //todo: добавить обновление вверх по иерархии на этапе обработки эвентов
            //добавить выборку, берем имя, по имени обращемся к коллекции

            var connection =  MainWindow.Instance.ConnectionManager.Connections.Find(i=>i.Name == selectedPortfolio.Connection.Name);
            if (connection != null)
            {
                if (connection.ConnectionState == ConnectionStates.Connected)
                {
                    if (SecurityPicker.SecurityProvider != null)
                    {
                        SecurityPicker.SecurityProvider.Securities.Clear();
                        SecurityPicker.SecurityProvider.Securities.AddRange(selectedPortfolio.Connection.Connection.Tools);
                    }
                    
                }
                else
                    SecurityPicker.SecurityProvider.Securities.Clear();
            }
            //if (selectedPortfolio != null) ToolComboBox.ItemsSource = selectedPortfolio.Connection.Connection.Tools;
        }

        private void AmountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Unit newVolume;
            try
            {
                if (!AmountTextBox.Text.IsEmpty())
                    newVolume = AmountTextBox.Text.ToUnit();
                else
                {
                    UnitVolumeLabel.Content = "";
                    return;
                }
            }
            catch (Exception)
            {
                UnitVolumeLabel.Content = "";
                return;
            }
            UnitVolumeLabel.Content = newVolume == null ? "" : (newVolume.Type == UnitTypes.Percent ? " от счёта" : " контрактов");
        }

        public string this[string columnName]
        {
            get
            {
                string validationResult = null;
                switch (columnName)
                {
                    case "Portfolio":
                        validationResult = ValidatePortfolio();
                        break;
                    case "GroupOrSingleAgent":
                        validationResult = ValidateAgentOrGroup();
                        break;
                    case "Tools":
                        validationResult = ValidateTools();
                        break;
                    case "Amount":
                        validationResult = ValidateAmount();
                        break;
                    default:
                        throw new ApplicationException("Unknown Property being validated on Product.");
                }
                string error = validationResult;
                validManagerProperties[columnName] = String.IsNullOrEmpty(error) ? true : false;
                if (validManagerProperties.Count == 4)
                    OkBtnClick.IsEnabled = validManagerProperties.Values.All(isValid => isValid);
                return validationResult;
            }
        }

        private string ValidatePortfolio()
        {
            if (String.IsNullOrEmpty(Portfolio))
                return "Не выбран портфель";
            return String.Empty;
        }
        private string ValidateAgentOrGroup()
        {
            //TODO: уточнить как обрабатывать уже добавленные агенты и группы агентов

            if (String.IsNullOrEmpty(GroupOrSingleAgent))
                return "Не выбран агент или группа агентов";
            return String.Empty;
        }
        private string ValidateTools()
        {
            //добавить уведомление если инструменты не получены, так как соединение не активно
            
            //if (ToolComboBox.SelectedItem == null)
            //{
            //    return "Не выбран инструмент";
            //}
            return String.Empty;
        }
        private string ValidateAmount()
        {
            //TODO: добавить обработку на знаки, которые не допустимы в данном поле
            //так же обработка случая, когда форма скрыта
            if(String.IsNullOrEmpty(Amount.ToString()))
                return "Не указан объем";
            return String.Empty;
        }

        private bool ValidateProperties()
        {
            return validManagerProperties.Values.All(isValid => isValid);
        }
        public string Error { get; private set; }

        private void ToolComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            //ToolComboBox.ItemsSource = MainWindow.Instance.SecuritiesList.Select(i => i.Id).ToList();

        }

        private void ToolComboBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SecurityPicker.SecurityProvider.Securities.AddRange(MainWindow.Instance.SecuritiesList);
            ShowDialog();
        }
    } 
}
