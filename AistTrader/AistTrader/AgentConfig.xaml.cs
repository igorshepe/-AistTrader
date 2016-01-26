using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Strategies.Common;
using Strategies.Settings;

namespace AistTrader
{
    #region Garbage
    //public class AgentValidationRule : ValidationRule
    //{
    //    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    //    {
    //        //if (value is ComboBoxItem)
    //        //TODO: вынести в отдельный блок верификации, использовать тут и в классе где контролы
    //        var result = new ValidationResult(false, "Выберете алгоритм");
    //        //если ничего не выбрано
    //        if (value == null)
    //        {

    //            return new ValidationResult(false, "Выберете алгоритм");

    //        }

    //        if (string.Equals(value,AgentValidationError.NameAndSettingsAlreadyExist))
    //        {
    //            return new ValidationResult(false, "Стратегий с таким именем и настройками уже зарегестрирована");
    //        }

    //        return new ValidationResult(false, null);
    //    }
    //}
    #endregion

    public partial class AgentConfig : IDataErrorInfo
    {
        #region Fields
        public string Strategy { get; set; }
        public SerializableDictionary<string, object> AgentSettings { get; set; }
        private int EditIndex { get; set; }
        private bool _alreadyExist;
        #endregion
        public AgentConfig()
        {
            DataContext = this;
            InitializeComponent();
            EditIndex = int.MinValue;
        }
        public AgentConfig(Agent agent, int editIndex)
        {
            InitializeComponent();
            InitFields(agent);
            EditIndex = editIndex;
            AgentSettings = agent._Agent.SettingsStorage;
        }
        private void InitFields(Agent agent)
        {
            AlgorithmComboBox.ItemsSource = HelperStrategies.GetStrategies().Select(type => type.Name).ToList();
            AlgorithmComboBox.SelectedItem = agent._Agent.Algorithm.ToString();
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
                var type = HelperStrategies.GetStrategySettingsType(AlgorithmComboBox.SelectedItem.ToString());
                object settingsClassInstance = Activator.CreateInstance(type);
                var strategyDs = (StrategyDefaultSettings)settingsClassInstance;
                var strategySw = new StrategiesSettingsWindow(AgentSettings, strategyDs);
                strategySw.Settings.Load(AgentSettings);
                AgentSettings = strategySw.Settings.Save();
                type = null;
                //todo:сделать уведомления в всплывающем окне с анимацией/запись лога
                //MessageBox.Show(this, @"Применены дефолтные настройки для выбранного алгоритма");
                strategySw.Close();
                strategySw = null;
                var selectedStrategy = AlgorithmComboBox.SelectedItem.ToString();
                if (MainWindow.Instance.AgentsStorage != null)
                {
                    var algorithmNameInCollection = MainWindow.Instance.AgentsStorage.Cast<Agent>().Any(i => i.Name.StartsWith(selectedStrategy));
                    if (algorithmNameInCollection)
                    {
                        foreach (var strategy in MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Name.StartsWith(selectedStrategy)))
                        {
                            if (strategy._Agent.SettingsStorage.SequenceEqual(AgentSettings))
                            {
                                _alreadyExist = true;
                            }
                            //todo: добавить рамку и выводить сообщение о том, что добавление невозможно
                        }
                    }
                    else
                    {
                        _alreadyExist = false;
                    }
                    
                }
                else
                {
                    _alreadyExist = false;
                }
            }
            else
            {

            }
            //if (AlgorithmComboBox.SelectedItem != null&& AlreadyExist)
            //    AlgorithmOkBtn.IsEnabled = true;
        }
        private void AlgorithmComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (AlgorithmComboBox.SelectedItem == null)
            {
                AlgorithmComboBox.ItemsSource = HelperStrategies.GetStrategies().Select(type => type.Name).ToList();
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
                var strategySw = new StrategiesSettingsWindow(AgentSettings, strategyDs);
                strategySw.Settings.Load(AgentSettings);
                AgentSettings = strategySw.Settings.Save();
                type = null;
                //TODO: всплывающее окно, о том, что применены по дефолту кастомное
                //MessageBox.Show(this, @"Применены дефолтные настройки для выбранного алгоритма");

                strategySw.Close();
                strategySw = null;
            }
            var selectedAlgorithmStr = AlgorithmComboBox.SelectedItem.ToString();
            var algorithm = HelperStrategies.GetStrategyFriendlyName(selectedAlgorithmStr, AgentSettings);
            var pickedAlgorithm = (PickedStrategy)Enum.Parse(typeof(PickedStrategy), selectedAlgorithmStr);
            var agent = new AlgorithmSettings(pickedAlgorithm, -1, true, -1, AgentSettings, selectedAlgorithmStr, 10);
            MainWindow.Instance.AddNewAgent(new Agent(algorithm, agent), EditIndex);
            Close();
        }
        private void AgentSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            if (AlgorithmComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, @"Не выбран алгоритм");
                return;
            }
            var type = HelperStrategies.GetStrategySettingsType(AlgorithmComboBox.SelectedItem.ToString());
            var iSettingsDlg = (StrategyDefaultSettings)Activator.CreateInstance(type);
            var vrsDialog = new StrategiesSettingsWindow(AgentSettings, iSettingsDlg);

            var dlgResults = vrsDialog.ShowDialog();
            //iSettingsDlg = null;
            //type = null;
            if (dlgResults.HasValue && dlgResults.Value)
            {
                AgentSettings = vrsDialog.SettingsStorage;
                UniqueStrategyNameReCheckAfterSettingsAltering(AgentSettings);
                //триппер тайм!
                int currentIndex = AlgorithmComboBox.SelectedIndex;

                //TODO: попробовать это
                //BindingExpression binding = txtWindowTitle.GetBindingExpression(TextBox.TextProperty);
                //binding.UpdateSource();
                //http://www.wpf-tutorial.com/data-binding/the-update-source-trigger-property/

                AlgorithmComboBox.SelectedIndex = 1;
                AlgorithmComboBox.SelectedIndex = currentIndex;
            }
        }
        private void UniqueStrategyNameReCheckAfterSettingsAltering(SerializableDictionary<string, object> sd)
        {
            //передать в форму
            _alreadyExist = false;
            var selectedStrategy = AlgorithmComboBox.SelectedItem.ToString();
            if (Settings.Default.Agents != null)
            {
                var algorithmNameInCollection = MainWindow.Instance.AgentsStorage.Cast<Agent>().Any(i => i.Name.StartsWith(selectedStrategy));
                if (algorithmNameInCollection)
                {
                    foreach (var strategy in MainWindow.Instance.AgentsStorage.Cast<Agent>().Where(i => i.Name.StartsWith(selectedStrategy)))
                    {
                        if (strategy._Agent.SettingsStorage.SequenceEqual(sd))
                            _alreadyExist = true;
                        #region used to be like..
                        //var errorMassage = "Стратегий с таким именем и настройками уже зарегестрирована";
                        //vError.Validate(AgentValidationError.NameAndSettingsAlreadyExist, CultureInfo.CurrentCulture);
                        #endregion
                        else
                            _alreadyExist = false;
                    }
                }
                else
                {
                    _alreadyExist = false;
                }
            }
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
                return "Не выбран алгоритм";
            }
            if (_alreadyExist)
            {
                _alreadyExist = false;
                AlgorithmOkBtn.IsEnabled = false;
                return "Стратегий с таким именем и настройками уже зарегестрирована";
            }
            //if (String.IsNullOrEmpty(AlgorithmComboBox.SelectedItem.ToString()))
            //else if (this.AlgorithmName.Length < 5)
            //    return "Product Name should have more than 5 letters.";
            //else
            AlgorithmOkBtn.IsEnabled = true;
            return String.Empty;
        }
        public string Error { get; private set; }
    }
}
