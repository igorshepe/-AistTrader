using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using StockSharp.Messages;

namespace AistTrader
{
    public partial class AgentManagerAddition:IDataErrorInfo
    {
        private Dictionary<string, bool> validManagerProperties = new Dictionary<string, bool>();

        private string _portfolio; 
        public string Portfolio 
        {
            get { return _portfolio; }
            set { _portfolio = value; } 

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


        public ObservableCollection<AgentManager> AgentManagerStorage { get; private set; }
        private int EditIndex { get; set; }
        public AgentManagerAddition()
        {
            
            AgentManagerStorage = new ObservableCollection<AgentManager>();
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }
        public AgentManagerAddition(AgentManager agent, int editIndex)
        {
            
            AgentManagerStorage = new ObservableCollection<AgentManager>();
            InitializeComponent();
            DataContext = this;
            InitFields(agent);
            EditIndex = editIndex;
        }
        private void LoadParams()
        {
            List<string> resultsList = Settings.Default.Agents.Cast<Agent>().Where(i => i._Agent.GroupName == "Without Group").Select(i => i.Name).ToList();
            var results = Settings.Default.Agents.Cast<Agent>().Where(i => i._Agent.GroupName != "Without Group").Select(i => i._Agent.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList;

            //var agents = Settings.Default.AgentManager.Cast<AgentManager>().Select(i => i.Name).ToList();
            //var accounts = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().Select(i =>i).Except(agents).ToList();
            var accounts = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().Select(i => i.Name).ToList();
            AccountComboBox.ItemsSource = accounts;
        }
        private void InitFields(AgentManager agent)
        {
            List<string> resultsList = Settings.Default.Agents.Cast<Agent>().Where(i => i._Agent.GroupName == "Without Group").Select(i => i.Name).ToList();
            var results = Settings.Default.Agents.Cast<Agent>().Where(i => i._Agent.GroupName != "Without Group").Select(i => i._Agent.GroupName).Distinct().ToList();
            resultsList.AddRange(results);
            GroupOrSingleAgentComboBox.ItemsSource = resultsList;
            GroupOrSingleAgentComboBox.SelectedItem = agent.AgentManagerSettings.AgentOrGroup;

            AccountComboBox.ItemsSource = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().Select(i => i.Name).ToList();
            AccountComboBox.SelectedItem = agent.Name;
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
            AgentManagerSettings setting;
            var agentPortfolio = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().FirstOrDefault(i => i.Name == AccountComboBox.SelectedItem.ToString());
            var agent = Settings.Default.Agents.Cast<Agent>().FirstOrDefault(i => i.Name == GroupOrSingleAgentComboBox.SelectedItem.ToString());
            if (agent == null)
            {
                agent = Settings.Default.Agents.Cast<Agent>().FirstOrDefault(i => i._Agent.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
                setting = new AgentManagerSettings(agentPortfolio, agent._Agent.GroupName);
            }
            else
                setting = new AgentManagerSettings(agentPortfolio, agent.Name);
            MainWindow.Instance.AddNewAgentManager(new AgentManager(setting.Account.Name , setting, "has yet to be", AmountTextBox.Value), EditIndex);
            Close();
        }


        private void GroupOrSingleAgentComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var result = Settings.Default.Agents.Cast<Agent>().Any
                (i => i._Agent.GroupName != "Without Group" && i._Agent.GroupName == GroupOrSingleAgentComboBox.SelectedItem.ToString());
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
            var selectedPortfolio = Settings.Default.AgentPortfolio.Cast<AgentPortfolio>().FirstOrDefault(i => i.Name == (string)AccountComboBox.SelectedItem);
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
            
            if (ToolComboBox.SelectedItem == null)
            {
                return "Не выбран инструмент";
            }
            return String.Empty;
        }
        private string ValidateAmount()
        {
            //TODO: добавить обработку на знаки, которые не допустимы в данном поле
            //так же обработка случая, когда форма скрыта
            if(String.IsNullOrEmpty(Amount))
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
            ToolComboBox.ItemsSource = MainWindow.Instance.SecuritiesList.Select(i => i.Id).ToList();

        }
    }
}
