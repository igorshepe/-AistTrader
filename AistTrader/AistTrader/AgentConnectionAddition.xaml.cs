using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Ecng.Common;

namespace AistTrader
{
    public partial class AgentConnectionAddition : IDataErrorInfo
    {
        #region Fields
        private int EditIndex { get; set; }
        private string _connectionName;
        private string _code;
        private string _providerPath;
        private string _selectedPath;
        public string ConnectionName
        {
            get { return _connectionName; }
            set { _connectionName = value; }
        }
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }
        public string ProviderPath
        {
            get { return _providerPath; }
            set { _providerPath = value; }
        }
        private Dictionary<string, bool> validProperties = new Dictionary<string, bool>();
        public string SelectedPath
        {
            get { return _selectedPath; }
            set { _selectedPath = value; }
        }
        public bool IsAdditionMode;
        public string Provider { get; set; }
        public bool NameAlredyInUse;
        public ObservableCollection<Agent> AgentStorage { get; private set; }
        #endregion
        public AgentConnectionAddition()
        {
            IsAdditionMode = true;
            DataContext = this;
            InitializeComponent();
            EditIndex = int.MinValue;
            AllPlazaDirectoriesComboBox.ItemsSource = GetAllPlazaDirectories();
        }
        private List<string> GetAllPlazaDirectories()
        {
            List<string> list = new List<string>();
            var directories = Directory.EnumerateDirectories(@"C:\").Where(i => i.Contains("SpectraCGate")).ToList();
            if (IsAdditionMode)
            {
                if (!MainWindow.Instance.ProviderStorage.IsNull())
                {
                    var alreadyUsedPlazaRouters = MainWindow.Instance.ProviderStorage.Cast<AgentConnection>().ToList();
                    var result = directories.Where(i => alreadyUsedPlazaRouters.All(i2 => i2.Connection.ConnectionSettings.Path != i)).ToList();
                    return result;
                }
            }
            list.AddRange(directories);
            return list; 
        }
        public AgentConnectionAddition(AgentConnection account, int editIndex)
        {
            IsAdditionMode = false;
            InitializeComponent();
            InitFields(account);
            DataContext = this;
            EditIndex = editIndex;
        }
        private void InitFields(AgentConnection account)
        {
            _connectionName = account.Connection.Name;
            _code = account.Connection.Code;
            Provider = account.Connection.ConnectionSettings.Type.ToString();
            //_providerPath= GetAllPlazaDirectories();
            AllPlazaDirectoriesComboBox.ItemsSource = GetAllPlazaDirectories();
            //AllPlazaDirectoriesComboBox.SelectedItem = account.Connection.ConnectionSettings.Path.ToString();
            _selectedPath = account.Connection.ConnectionSettings.Path;
        }
        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            //    if (!File.Exists(AllPlazaDirectoriesComboBox.SelectedItem + @"\client_router.ini"))
            //    {
            //        //TODO: проверка на наличие роутера
            //        MessageBox.Show(this, @"В выбранной дериктории нет ini файла: {0}.".Put(AllPlazaDirectoriesComboBox.SelectedItem));
            //        return;
            //    }
            var terminalConnSettings = new TerminalSettings((TerminalType)ConnectionTypeComboBox.SelectedItem, AllPlazaDirectoriesComboBox.SelectedItem.ToString());
            var agent = new ConnectionsSettings(ClienNameTxtBox.Text, ClienCodeTxtBox.Text, terminalConnSettings, false);
            MainWindow.Instance.AddNewAgentConnection(new AgentConnection(ClienNameTxtBox.Text, agent), EditIndex);
            Close();
        }
        private List<string> GetAgents()
        {
            var list = new List<string>();
            AgentStorage = new ObservableCollection<Agent>();
            if (Settings.Default.Agents != null)
                try
                {
                    foreach (var rs in Settings.Default.Agents.Cast<Agent>())
                    {
                        AgentStorage.Add(rs);
                    }
                    if (AgentStorage != null && AgentStorage.Count > 0)
                    {
                        list.AddRange(AgentStorage.Select(i => i.Name));
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(this, @"Не удалось прочитать настройки. Задайте заново.");
                    Settings.Default.Agents.Clear();
                }

            return list;
        }
        private void ClienNameTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            //string result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ClienNameTxtBox.Text);
            //ClienNameTxtBox.Text = result;
            //ClienNameTxtBox.SelectionStart = ClienNameTxtBox.Text.Length;
        }
        private void BtnSelectPath_OnClick(object sender, RoutedEventArgs e)
        {
            //using (var dlg = new FolderBrowserDialog())
            //{
            //    if(!PathToRouter.Text.IsEmpty())
            //        dlg.SelectedPath = PathToRouter.Text;
            //    else
            //        dlg.SelectedPath = @"C:\P2FORTSGate\";

            //    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        PathToRouter.Text = dlg.SelectedPath;
            //    }
            //}
        }
        private void ClienNameTxtBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            //ЗАЧЕМ ЭТО?
            //var has = Settings.Default.AgentConnection.Cast<AgentConnection>().Any(i => i.Name == ClienNameTxtBox.Text);
        }
        public string this[string columnName]
        {
            get
            {
                string validationResult = null;
                switch (columnName)
                {
                    case "ConnectionName":
                        validationResult = ValidateName();
                        break;
                    case "Code":
                        validationResult = ValidateCode();
                        break;
                    //case "Provider":
                    //    validationResult = ValidateProvider();
                    //    break;
                    case "ProviderPath":
                        validationResult = ValidateProviderPath();
                        break;
                    default:
                        throw new ApplicationException("Unknown Property being validated on Product.");
                }
                string error = validationResult;
                validProperties[columnName] = String.IsNullOrEmpty(error) ? true : false;
                if (validProperties.Count == 3)
                    OkAgentConnectionBtn.IsEnabled = validProperties.Values.All(isValid => isValid);    
                return validationResult;
            }
        }
        private string ValidateName()
        {
            if (String.IsNullOrEmpty(this.ConnectionName))
                return "Задайте имя";
            else if (this.ConnectionName.Length < 5)
                return "Имя должно содержать не меньше 5 символов.";
            else if (NameAlredyInUse)
                return "Данное имя уже используется";
            else
                return String.Empty;
        }
        private string ValidateCode()
        {
            if (String.IsNullOrEmpty(this.Code))
                return "Задайте код";
            else if (this.Code.Length < 2)
                return "Код должен содержать не меньше 2х символов";
            else
                return String.Empty;
        }
        private string ValidateProvider()
        {
            if (ConnectionTypeComboBox.SelectedItem == null)
            {
                return "Не выбран поставщик";
            }
            return String.Empty;
        }
        private string ValidateProviderPath()
        {
            if (AllPlazaDirectoriesComboBox.SelectedItem == null)
            {
                return "Не выбран путь к роутеру";
            }
            if (AllPlazaDirectoriesComboBox != null)
            {
                if (!File.Exists(AllPlazaDirectoriesComboBox.SelectedItem + @"\client_router.ini"))
                {
                    return  string.Format("В выбранной дериктории нет ini файла: {0}".Put(AllPlazaDirectoriesComboBox.SelectedItem)) ;
                }    
            }
            return String.Empty;
        }
        private bool ValidateProperties()
        {
            return validProperties.Values.All(isValid => isValid);
        }
        public string Error { get; private set; }
        //todo: проверить что делает этот метод, поменять ресурс 
        private void ClienNameTxtBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsAdditionMode)
            {
                var text = ClienNameTxtBox.Text;
                if (Settings.Default.AgentConnection != null)
                    NameAlredyInUse = Settings.Default.AgentConnection.Cast<AgentConnection>().Any(i => i.Name == text);    
            }
        }
    }
}
