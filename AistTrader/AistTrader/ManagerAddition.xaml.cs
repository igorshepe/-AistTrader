using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using StockSharp.Algo;
using StockSharp.Algo.Storages;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    public partial class ManagerAddition : IDataErrorInfo
    {
        private bool editMode;
        private AgentManager agentManagerToEdit;
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
        private string _amount;
        public string Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }
        private string _alias;
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }
        private bool IsGroup;
        private int EditIndex { get; set; }
        public ManagerAddition()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }
        public ManagerAddition(AgentManager agent, int editIndex)
        {
            InitializeComponent();
            DataContext = this;
            InitFields(agent);
            EditIndex = editIndex;
        }
        private void LoadParams()
        {
            //todo:выбирать для каждого подключения/портфеля свой набор параметров
            List<string> resultsList = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName).ToList();
            var results = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName != "ungrouped agents").Select(i => i.Params.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList;
            resultsList = null;
            var accounts = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().Select(i => i.Name).ToList();
            PortfolioComboBox.ItemsSource = accounts;
            accounts = null;
            AmountTextBox.Text = "";
        }
        private void InitFields(AgentManager agent)
        {
            editMode = true;
            agentManagerToEdit = agent;

            //SecurityPickerSS.SecurityProvider = new FilterableSecurityProvider(MainWindow.Instance.ConnectionManager.Connections[0]);
            PortfolioComboBox.ItemsSource = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().Select(i => i.Name).ToList();
            _selectedPortfolio = agent.Name;

            List<string> resultsList = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName).ToList();
            var results = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName != "ungrouped agents").Select(i => i.Params.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList.ToList();
            _selectedGroupOrSingleAgent = agent.AgentManagerSettings.AgentOrGroup;

            string selectedP = (string)_selectedPortfolio;
            var selectedPortfolio = MainWindow.Instance.AgentPortfolioStorage.FirstOrDefault(i => i.Name == selectedP);
            GroupOrSingleAgentComboBox.SelectedItem = _selectedGroupOrSingleAgent;
            var connection = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.Id == selectedPortfolio.Connection.Id);
            var conn = MainWindow.Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == connection.DisplayName);
            SecurityPickerSS.SecurityProvider = new CollectionSecurityProvider(conn.Securities);
            SecurityPickerSS.SelectedSecurity = conn.Securities.FirstOrDefault(i=>i.Code == agent.Tool);
            conn = null;
            _amount = agent.Amount.ToString();
            if (agent.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Running)
            {
                GroupOrSingleAgentComboBox.IsEnabled = false;
                PortfolioComboBox.IsEnabled = false;
                AliasTxtBox.IsEnabled = false;
                SecurityPickerSS.IsEnabled = false;
                editMode = true;
            }
            
        }
        private void AddAgentInAgentManagerBtnClick(object sender, RoutedEventArgs e)
        {
            if (AliasTxtBox.Text== "")
            {
                MessageBox.Show(this, @"Set an alias");
                return;
            }
            if (SecurityPickerSS.SelectedSecurity == null)
            {
                MessageBox.Show(this, @"Select a security");
                return;
            }

            //временная проверка не через автовалидацию
            if (editMode)
            {


                if (agentManagerToEdit.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Running)
                {
                    agentManagerToEdit.Amount = AmountTextBox.Text;
                    MainWindow.Instance.AddNewAgentManager(agentManagerToEdit, EditIndex);
                    Close();
                }
                else
                {
                    agentManagerToEdit.Alias = AliasTxtBox.Text;
                    MainWindow.Instance.AddNewAgentManager(agentManagerToEdit, EditIndex);
                    agentManagerToEdit = null;
                    Close();
                }
                
                editMode = false;
            }
            else
            {
                if (MainWindow.Instance.AgentManagerStorage.Any(i => i.Alias == AliasTxtBox.Text))
                {
                    MessageBox.Show(this, @"This alias already in use");
                    return;
                }
            }

            ManagerParams setting;
            var agentPortfolio = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().FirstOrDefault(i => i.Name == PortfolioComboBox.SelectedItem.ToString());
            var agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().FirstOrDefault(i => i.Params.FriendlyName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
            if (agent == null)
            {
                agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().FirstOrDefault(i => i.Params.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
                setting = new ManagerParams(agentPortfolio, agent.Params.GroupName, SecurityPickerSS.SelectedSecurity.Code);
            }
            else

                setting = new ManagerParams(agentPortfolio, agent.Params.FriendlyName, SecurityPickerSS.SelectedSecurity.Code);


            if (AmountTextBox.Text == "" && agent.Params.GroupName =="")
            {
                MessageBox.Show(this, @"Set amount value");
                return;
            }
            if (AmountTextBox.Text == "" && agent.Params.GroupName == "ungrouped agents")
            {
                MessageBox.Show(this, @"Set amount value");
                return;
            }

            MainWindow.Instance.AddNewAgentManager(new AgentManager(setting.Portfolio.Name , setting, setting.Tool,AmountTextBox.Text, AliasTxtBox.Text), EditIndex);
            SecurityPickerSS.SecurityProvider.Dispose();
            SecurityPickerSS.SecurityProvider = null;
            agentPortfolio = null;
            Close();
        }
        private void GroupOrSingleAgentComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupOrSingleAgentComboBox.SelectedItem !=null)
            {
                var result = MainWindow.Instance.AgentsStorage.Cast<Agent>().Any
                (i => i.Params.GroupName != "ungrouped agents" && i.Params.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
                if (result)
                {
                    AmountTextBox.Visibility = Visibility.Collapsed;
                    AmountLbl.Visibility = Visibility.Collapsed;
                    IsGroup = true;
                }
                else
                {
                    IsGroup = false;
                    AmountTextBox.Visibility = Visibility.Visible;
                    AmountLbl.Visibility = Visibility.Visible;
                }
                AliasTxtBox.Text = GroupOrSingleAgentComboBox.SelectedItem.ToString();
            }
        }
        private void PortfolioComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedP = (string)PortfolioComboBox.SelectedItem;
            var selectedPortfolio = MainWindow.Instance.AgentPortfolioStorage.FirstOrDefault(i => i.Name == selectedP);
            GroupOrSingleAgentComboBox.SelectedItem = _selectedGroupOrSingleAgent;
            var connection =  MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i=>i.Id == selectedPortfolio.Connection.Id);
            
            if (connection != null)
            {
                if (connection.ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Connected)
                {
                    if (SecurityPickerSS.SecurityProvider != null)
                    {
                    }
                    else
                    {
                        var conn= MainWindow.Instance.ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == connection.DisplayName);
                        SecurityPickerSS.SecurityProvider=  new CollectionSecurityProvider(conn.Securities);

                        conn = null;
                    }
                }
            }
            selectedPortfolio = null;
            connection = null;
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
                {
                    if (SecurityPickerSS.SelectedSecurity != null)
                    {
                        var props= validManagerProperties;
                        validManagerProperties.Remove("Tools");
                    }
                    if (AmountTextBox.Visibility == Visibility.Collapsed)
                    {
                        validManagerProperties.Remove("Amount");
                    }
                    OkBtnClick.IsEnabled = true; /* validManagerProperties.Values.All(isValid => isValid)*/
                    //todo: подключить после тестов;
                }
                if (validManagerProperties.Count == 3 & AmountTextBox.Visibility != Visibility.Collapsed)
                {
                    if (SecurityPickerSS.SelectedSecurity != null)
                    {
                        var props = validManagerProperties;
                        validManagerProperties.Remove("Tools");
                        validManagerProperties.Remove("Amount");
                    }
                    OkBtnClick.IsEnabled = validManagerProperties.Values.All(isValid => isValid);
                }

                return validationResult;
            }
        }

        private string ValidatePortfolio()
        {
            if (String.IsNullOrEmpty(Portfolio))
                return "Select a portfolio";
            return String.Empty;
        }
        private string ValidateAgentOrGroup()
        {
            //TODO: уточнить как обрабатывать уже добавленные агенты и группы агентов

            if (String.IsNullOrEmpty(GroupOrSingleAgent))
                return "Select an agent or a group of agents";
            return String.Empty;
        }
        private string ValidateTools()
        {
            if (SecurityPickerSS.SelectedSecurity == null)
            {
                return "Select a security";
            }
            return String.Empty;
        }
        private string ValidateAmount()
        {
            Regex regex = new Regex(@"^[0-9]+$");
            if (Amount != null && Amount.EndsWith("%"))
            {
                string[] line = Amount.Split('%');
                if (!regex.IsMatch(line.First()))
                {
                    Amount= "";
                    return "Digits only or digits with - '%' sign at the end allowed";
                }
            }
            if (Amount != null && !Amount.EndsWith("%"))
            {
                if (!regex.IsMatch(Amount))
                {
                    Amount = "";
                    return "Digits only or digits with - '%' sign at the end allowed";
                }
            }
            //TODO: добавить обработку на знаки, которые не допустимы в данном поле
            //так же обработка случая, когда форма скрыта
            if (String.IsNullOrEmpty(Amount))
                return "Set an amount";
            return String.Empty;
        }
        public string Error { get; private set; }
        private void AmountTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            var editor = sender as UnitEditor;
            if (editor.Text.EndsWith("%"))
            {
                string[] line = editor.Text.Split('%');
                if (!regex.IsMatch(line.First()))
                {
                    //MessageBox.Show("Возможет ввод только цифр или цифры со знаком % на конце");
                    editor.Text = "";
                    return;
                    // editor.Select(editor.Text.Length, 0);
                }
            }
            if (!editor.Text.EndsWith("%"))
            {
                if (!regex.IsMatch(editor.Text))
                {
                    //MessageBox.Show("Возможет ввод только цифр или цифры со знаком % на конце");
                    editor.Text = "";
                    return;
                    //editor.Select(editor.Text.Length, 0);
                }
            }
        }
        private void SecurityPicker_OnSecuritySelected()
        {
            if (SecurityPickerSS.SelectedSecurity != null && PortfolioComboBox.SelectedItem != null && IsGroup)
            {
                OkBtnClick.IsEnabled = true;
            }
        }
    } 
}
