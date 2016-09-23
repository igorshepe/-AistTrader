using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Common.Entities;
using Common.Params;
using Strategies.Common;
using Strategies.Settings;

namespace AistTrader
{
    public partial class AgentAddition : IDataErrorInfo
    {
        #region Fields
        public string Strategy { get; set; }
        public SerializableDictionary<string, object> AgentSettings { get; set; }
        private int EditIndex { get; set; }
        private bool _alreadyExist;
        private ObservableCollection<object> AgentNameSettings { get; set; }

        #endregion
        public AgentAddition()
        {
            DataContext = this;
            InitializeComponent();
            EditIndex = int.MinValue;
        }

        public AgentAddition(Agent agent, int editIndex)
        {
            InitializeComponent();
            InitFields(agent);
            EditIndex = editIndex;
            AgentSettings = agent.Params.SettingsStorage;
        }

        private void InitFields(Agent agent)
        {
            AlgorithmComboBox.ItemsSource = HelperStrategies.GetStrategies().Select(type => type.Name).ToList();
            AlgorithmComboBox.SelectedItem = agent.Params.FriendlyName.ToString();
        }
        private void AlgorithmComboBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            //todo:вынести логику в отдельный класс
            if (AlgorithmComboBox.SelectedItem == null)
            {
                AgentSettingsButton.IsEnabled = false;
                return;
            }
            if (AlgorithmComboBox.SelectedIndex == -1)
            {
                AgentSettingsButton.IsEnabled = false;
                return;
            }
            var hasSetting = HelperStrategies.StrategyHasParams(AlgorithmComboBox.SelectedItem.ToString());
            AgentSettingsButton.IsEnabled = hasSetting;
            if (hasSetting)
            {
                //есть настройки
                //создаем инстанс той стратеги, которую пишем в систему
                var type = HelperStrategies.GetStrategySettingsType(AlgorithmComboBox.SelectedItem.ToString());
                object settingsClassInstance = Activator.CreateInstance(type);
                var agentDefaultSettings = (StrategyDefaultSettings)settingsClassInstance;
                var agentSettingWindow = new AgentSettings(AgentSettings, agentDefaultSettings);
                if (AgentSettings == null)
                {
                    agentSettingWindow.Settings.Load(AgentSettings);
                    AgentSettings = agentSettingWindow.SettingsStorage;
                }
                AgentSettings = agentSettingWindow.SettingsStorage;

                var agentPotentialNameStr = HelperStrategies.GetStrategyFriendlyName(AlgorithmComboBox.SelectedItem.ToString(), AgentSettings);
                AgentNameSettings = new ObservableCollection<object>();
                AgentNameSettings = AistTrader.AgentSettings.AgentSettingsStorage;
                type = null;
                //todo:сделать уведомления в всплывающем окне с анимацией/запись лога
                agentSettingWindow.Close();
                agentSettingWindow = null;
                var selectedStrategy = AlgorithmComboBox.SelectedItem.ToString();
                if (MainWindow.Instance.AgentsStorage != null)
                {
                    _alreadyExist = MainWindow.Instance.AgentsStorage.Cast<Agent>().Any(i => i.Name == agentPotentialNameStr);
                }
                else
                {
                    _alreadyExist = false;
                }
            }
            else
            {
                //todo: узнать про стратегии с вшитыми параметрами, дописать логику валидации под них сюда
            }
        }

        private void AlgorithmComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (AlgorithmComboBox.SelectedItem == null)
            {
                AlgorithmComboBox.ItemsSource  =  HelperStrategies.GetStrategies().Select(type => type.Name).ToList();
                AlgorithmOkBtn.IsEnabled = false;
            }
        }

        private void AddConfigBtnClick(object sender, RoutedEventArgs e)
        {
            if (AgentSettings == null && AgentSettingsButton.IsEnabled)
            {
                //TODO: добавить поток при вызове окна, занулить после обработки
                var type = HelperStrategies.GetStrategySettingsType(AlgorithmComboBox.SelectedItem.ToString());
                object settingsClassInstance = Activator.CreateInstance(type);
                var strategyDs = (StrategyDefaultSettings)settingsClassInstance;
                var strategySw = new AgentSettings(AgentSettings, strategyDs);
                strategySw.Settings.Load(AgentSettings);
                AgentSettings = strategySw.SettingsStorage;
                type = null;
                //TODO: всплывающее окно, о том, что применены по дефолту кастомное
                strategySw.Close();
                strategySw = null;
            }
             
            var agentFullName = HelperStrategies.GetStrategyFriendlyName(AlgorithmComboBox.SelectedItem.ToString(), AgentSettings);
            var exist = MainWindow.Instance.AgentsStorage.Any(i => i.Name == agentFullName);
            if (exist)
            {
                MessageBoxResult result = MessageBox.Show("Agent with these trade settings already registred! Consider changing trade settings & try again", "Uniqueness error", MessageBoxButton.OK);
                return;
            }

            var toolTipName = new StringBuilder();
            foreach (var param in AgentSettings)
            {
                toolTipName.Append(param.Key + "-" + param.Value + ",");
            }
            --toolTipName.Length;
            var agentCompiledName = new StringBuilder(AlgorithmComboBox.SelectedItem.ToString());

            foreach (AgentSettingParameterProperty set in AistTrader.AgentSettings.AgentSettingsStorage)
            {
                if (set.UseInAgentName)
                {
                    agentCompiledName.Append("_"+set.Parametr);
                }
            }
            var phantomParams = new AgentPhantomParams(agentFullName,null,null);
            var agentParams = new AgentParams(agentFullName, -1, -1, AgentSettings, AlgorithmComboBox.SelectedItem.ToString(), agentCompiledName.ToString(),toolTipName.ToString(), phantomParams);
            agentParams.TransactionId = new List<long>();
            MainWindow.Instance.AddNewAgent(new Agent(agentFullName, agentParams), EditIndex);
            agentCompiledName.Clear();
            Close();
        }

        private void AgentSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            if (AlgorithmComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, @"Script is not set");
                return;
            }
            var type = HelperStrategies.GetStrategySettingsType(AlgorithmComboBox.SelectedItem.ToString());
            var iSettingsDlg = (StrategyDefaultSettings)Activator.CreateInstance(type);
            var vrsDialog = new AgentSettings(AgentSettings, iSettingsDlg);

            var dlgResults = vrsDialog.ShowDialog();
            if (dlgResults.HasValue && dlgResults.Value)
            {
                AgentSettings = vrsDialog.SettingsStorage;
                UniqueStrategyNameReCheckAfterSettingsAltering(AgentSettings);
            }
        }

        private void UniqueStrategyNameReCheckAfterSettingsAltering(SerializableDictionary<string, object> sd)
        {
            var agentPotentialNameStr = HelperStrategies.GetStrategyFriendlyName(AlgorithmComboBox.SelectedItem.ToString(), sd);
            if (MainWindow.Instance.AgentsStorage.Cast<Agent>().Any(i => i.Name == agentPotentialNameStr))
            {
                _alreadyExist = true;
            }
            string error = (this as IDataErrorInfo)["Strategy"];

            //передать в форму
            var selectedStrategy = AlgorithmComboBox.SelectedItem.ToString();
        }

        public string this[string columnName]
        {
            get
            {   
                string validationResult = null;
                switch (columnName)
                {
                    case "Strategy":
                        validationResult = ValidateName();
                        break;
                    default:
                        throw new ApplicationException("Unknown Property being validated on Product.");
                }
                return validationResult;
            }
        }

        private string ValidateName()
        {
            if (AlgorithmComboBox.SelectedItem == null)
            {
                return "Script is not set";
            }
            if (_alreadyExist)
            {
                _alreadyExist = false;
                AlgorithmOkBtn.IsEnabled = true;

                return string.Empty;
            }
            AlgorithmOkBtn.IsEnabled = true;
            return string.Empty;
        }

        public string Error { get; private set; }
    }
}
