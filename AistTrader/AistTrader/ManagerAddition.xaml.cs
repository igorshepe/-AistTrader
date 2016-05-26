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
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    public partial class ManagerAddition : IDataErrorInfo
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
        private string _amount;
        public string Amount
        {
            get { return _amount; }
            set { _amount = value; }

        }

        private bool IsGroup;
        private int EditIndex { get; set; }
        public ManagerAddition()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
            
            //workin'
            //SecurityPicker.SecurityProvider.Securities.AddRange(MainWindow.Instance.SecuritiesList);

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
            //SecurityPicker.SecurityProvider = new FilterableSecurityProvider(MainWindow.Instance.ConnectionManager.Connections[0]);






            List<string> resultsList = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName).ToList();
            var results = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName != "ungrouped agents").Select(i => i.Params.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList;

            //var agents = Settings.Default.AgentManager.Cast<AgentManager>().Select(i => i.Name).ToList();
            //var accounts = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().Select(i =>i).Except(agents).ToList();
            var accounts = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().Select(i => i.Name).ToList();
            PortfolioComboBox.ItemsSource = accounts;
            AmountTextBox.Text = "";
        }
        private void InitFields(AgentManager agent)
        {
            SecurityPicker.SecurityProvider = new FilterableSecurityProvider(MainWindow.Instance.ConnectionManager.Connections[0]);
            PortfolioComboBox.ItemsSource = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().Select(i => i.Name).ToList();
            _selectedPortfolio = agent.Name;

            List<string> resultsList = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName).ToList();
            var results = MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Params.GroupName != "ungrouped agents").Select(i => i.Params.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList.ToList();
            _selectedGroupOrSingleAgent = agent.AgentManagerSettings.AgentOrGroup;



            SecurityPicker.SelectedSecurity = agent.Tool;

            _amount = agent.Amount.ToString();

            //AmountTextBox.Text = agent.Amount.ToString();
        }
        private void AddAgentInAgentManagerBtnClick(object sender, RoutedEventArgs e)
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
            ManagerParams setting;
            var agentPortfolio = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().FirstOrDefault(i => i.Name == PortfolioComboBox.SelectedItem.ToString());
            var agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().FirstOrDefault(i => i.Params.FriendlyName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
            if (agent == null)
            {
                agent = MainWindow.Instance.AgentsStorage.Cast<Agent>().FirstOrDefault(i => i.Params.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
                setting = new ManagerParams(agentPortfolio, agent.Params.GroupName, SecurityPicker.SelectedSecurity);
            }
            else
                setting = new ManagerParams(agentPortfolio, agent.Params.FriendlyName, SecurityPicker.SelectedSecurity);
            MainWindow.Instance.AddNewAgentManager(new AgentManager(setting.Portfolio.Name , setting, setting.Tool,AmountTextBox.Text), EditIndex);
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
            }

            
        }

        private void PortfolioComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPortfolio = MainWindow.Instance.AgentPortfolioStorage.Cast<Common.Entities.Portfolio>().FirstOrDefault(i => i.Name == (string)PortfolioComboBox.SelectedItem);
            GroupOrSingleAgentComboBox.SelectedItem = _selectedGroupOrSingleAgent;
            ////имя счета
            //var item = AccountComboBox.SelectedItem.ToString();

            //var portfolio = MainWindow.Instance.AgentPortfolioStorage.FirstOrDefault(i => i.Name == item);
            //item = item.Substring(0, item.IndexOf(" (", StringComparison.Ordinal));
            //var agent = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.Name == item);
            //var securities = selectedPortfolio.Connection.Connection.Tools;
            //List<Security> portfoliosList = new List<Security>();
            //foreach (var i in securities)
            //{
            //    portfoliosList.Add(i);
            //}

            //todo: добавить обновление вверх по иерархии на этапе обработки эвентов
            //добавить выборку, берем имя, по имени обращемся к коллекции
            //var connection = MainWindow.Instance.ConnectionManager.Connections.Find(i => i. == selectedPortfolio.Connection.Id);

            var connection =  MainWindow.Instance.ConnectionsStorage.First(i=>i.Id == selectedPortfolio.Connection.Id);
            if (connection != null)
            {
                if (connection.ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Connected)
                {
                    if (SecurityPicker.SecurityProvider != null)
                    {
                        //SecurityPicker.SecurityProvider.Securities.Clear();
                        //SecurityPicker.SecurityProvider.Securities.AddRange(selectedPortfolio.Connection.ConnectionParams.Tools);
                    }
                    else
                    {
                        //SecurityPicker.SecurityProvider = new FilterableSecurityProvider(MainWindow.Instance.ConnectionManager.Connections[0]);
                        SecurityPicker.SecurityProvider=  new FilterableSecurityProvider(MainWindow.Instance.ConnectionManager.Connections.FirstOrDefault(i=>i.ConnectionName == connection.DisplayName));
                    }
                }
                //else
                    //SecurityPicker.SecurityProvider.Securities.Clear();
            }
            //if (selectedPortfolio != null) ToolComboBox.ItemsSource = selectedPortfolio.Connection.Connection.Tools;
        }

        //private void AmountTextBox_KeyUp(object sender, KeyEventArgs e)
        //{

        //    Regex regex = new Regex(@"^[0-9]+$");
        //    var editor = sender as UnitEditor;
        //    if (editor.Text.EndsWith("%"))
        //    {
        //        string[] line = editor.Text.Split('%');
        //        if (!regex.IsMatch(line.First()))
        //        {
        //            //MessageBox.Show("Возможет ввод только цифр или цифры со знаком % на конце");
        //            _amount= editor.Text = "";
        //            return;
        //            // editor.Select(editor.Text.Length, 0);
        //        }
        //    }
        //    if (!editor.Text.EndsWith("%"))
        //    {
        //        if (!regex.IsMatch(editor.Text))
        //        {
        //            //MessageBox.Show("Возможет ввод только цифр или цифры со знаком % на конце");
        //            _amount= editor.Text = "";
        //            return;
        //            //editor.Select(editor.Text.Length, 0);
        //        }
        //    }




        //    //Unit newVolume;
        //    //try
        //    //{
        //    //    if (!AmountTextBox.Text.IsEmpty())
        //    //        newVolume = AmountTextBox.Text.ToUnit();
        //    //    else
        //    //    {
        //    //        UnitVolumeLabel.Content = "";
        //    //        return;
        //    //    }
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    UnitVolumeLabel.Content = "";
        //    //    return;
        //    //}
        //    //UnitVolumeLabel.Content = newVolume == null ? "" : (newVolume.Type == UnitTypes.Percent ? " от счёта" : " контрактов");
        //}

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
                    if (SecurityPicker.SelectedSecurity != null)
                    {
                        var props= validManagerProperties;
                        validManagerProperties.Remove("Tools");
                    }
                    if (AmountTextBox.Visibility == Visibility.Collapsed)
                    {
                        validManagerProperties.Remove("Amount");
                    }
                    OkBtnClick.IsEnabled = validManagerProperties.Values.All(isValid => isValid);
                }
                if (validManagerProperties.Count == 3 & AmountTextBox.Visibility != Visibility.Collapsed)
                {
                    if (SecurityPicker.SelectedSecurity != null)
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
            if (SecurityPicker.SelectedSecurity == null)
            {
                return "Не выбран инструмент";
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
                    return "Возможен ввод только цифр или цифры со знаком % на конце";
                }
            }
            if (Amount != null && !Amount.EndsWith("%"))
            {
                if (!regex.IsMatch(Amount))
                {
                    Amount = "";
                    return "Возможен ввод только цифр или цифры со знаком % на конце";
                }
            }

            //TODO: добавить обработку на знаки, которые не допустимы в данном поле
            //так же обработка случая, когда форма скрыта
            if (String.IsNullOrEmpty(Amount))
                return "Не указан объем";
            return String.Empty;
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

            //throw new NotImplementedException();
        }

        private void SecurityEditor_OnSecuritySelected(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            //Tools = SecurityPicker.SelectedSecurity.Name;
            //throw new NotImplementedException();
        }

        private void SecurityPicker_OnSecuritySelected()
        {
            if (SecurityPicker.SelectedSecurity != null && PortfolioComboBox.SelectedItem != null && IsGroup)
            {
                OkBtnClick.IsEnabled = true;
            }
        }
    } 
}
