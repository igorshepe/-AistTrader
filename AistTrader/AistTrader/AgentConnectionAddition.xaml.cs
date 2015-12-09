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
        private int EditIndex { get; set; }


        private string _connectionName;
        private string _code;
        private string _providerPath;
        private string _selectedPath; 

        public string ConnectionName 
        {
            get { return _connectionName; }
            set {_connectionName = value ; } 

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
            set { _selectedPath= value; }
        }
        public bool IsAdditionMode;
        public string Provider { get; set; }
        public bool NameAlredyInUse;
        public ObservableCollection<Agent> AgentStorage { get; private set; }

        public  void LoadValidationDictionary()
        {
            //validProperties.Add("ConnectionName", false);
            //validProperties.Add("ConnectionName", false);
            //validProperties.Add("ProviderPath", false);
        }

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
            //var directories = Directory.EnumerateDirectories(@"C:\").Where(i => i.Contains("P2FORTSGate")).ToList();
            var directories = Directory.EnumerateDirectories(@"C:\").Where(i => i.Contains("SpectraCGate")).ToList();
            if (IsAdditionMode)
            {
                
                if (!Settings.Default.AgentConnection.IsNull())
                {
                    var alreadyUsedPlazaRouters = Settings.Default.AgentConnection.Cast<AgentConnection>().ToList();
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
            //QuikPath.Text = account.Connection.ConnectionSettings.QuikPath;
            //Trans2QuikName.Text = account.Connection.ConnectionSettings.Trans2Quik;
            ////// PathToRouter.Text = account.Connection.ConnectionSettings.Path;
            //ConnectionTypeComboBox.Text = account.AgentAccount.ConnectionType;
            //LoginTxtBox.Text = account.Connection.Login;
            //PasswordTxtBox.Password = account.Connection.Password;
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {


            //if (!(ConnectionTypeComboBox.SelectedItem is TerminalType))
            //{
            //    MessageBox.Show(this, @"Не удалось получить терминал.");
            //    return;
            //}

            //var connectionType = (TerminalType)ConnectionTypeComboBox.SelectedItem;
            //if (ClienNameTxtBox.Text.Length <= 0)
            //{
            //    MessageBox.Show(this, @"Не задано имя.");
            //    return;
            //}

            //if (ClienCodeTxtBox.Text.Length <= 0)
            //{
            //    MessageBox.Show(this, @"Не задан код.");
            //    return;
            //}

            //if (connectionType == TerminalType.Plaza)
            //{
            //    //if (PathToRouter.Text.Length <= 0)
            //    //{
            //    //    MessageBox.Show(this, @"Не задан путь к Plaza - обязательное поле для {0}.".Put(connectionType));
            //    //    return;
            //    //}
            //    if (!File.Exists(AllPlazaDirectoriesComboBox.SelectedItem + @"\client_router.ini"))
            //    {
            //        //TODO: проверка на наличие роутера
            //        MessageBox.Show(this, @"В выбранной дериктории нет ini файла: {0}.".Put(AllPlazaDirectoriesComboBox.SelectedItem));
            //        return;
            //    }
            //}

            var terminalConnSettings = new TerminalSettings((TerminalType)ConnectionTypeComboBox.SelectedItem, AllPlazaDirectoriesComboBox.SelectedItem.ToString());
            var agent = new ConnectionsSettings(ClienNameTxtBox.Text, ClienCodeTxtBox.Text, terminalConnSettings, false);
            var t = new AgentConnection(ClienNameTxtBox.Text, agent);
            MainWindow.Instance.AddNewAgentConnection(new AgentConnection(ClienNameTxtBox.Text, agent), EditIndex);
            Close();
        }




        //private bool SuitsIndex(AccountAgent qStruct)
        //{
        //    return GetEditedItem(EditIndex) != qStruct;
        //}

        //private static AccountAgent GetEditedItem(int index)
        //{
        //    if (index < 0 || index >= ProviderManagerForm.Instance.AgentAccountStorage.Count) return null;

        //    return ProviderManagerForm.Instance.AgentAccountStorage[index];
        //}




        private void TerminalComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
                    Settings.Default.AistTrader.Clear();
                }

            return list;

        }

        //private SerializableDictionary<string, object> GetAlgotithmSettings(string algorithmName)
        //{
        //    Settings = AgentAccountStorage.Where(i => i.Name == algorithmName)
        //                    .Select(i => i.AgentAccountSettings.SettingsStorage)
        //                    .FirstOrDefault();
        //    return Settings;
        //}

        //private string GetAlgorithm(string agentName)
        //{
        //    //string algorithm="";
        //    //if (AgentAccountStorage != null && AgentAccountStorage.Count > 0)
        //    //{
        //    //    algorithm = AgentAccountStorage.Where(i => i.Name == agentName).Select(i => i._Agent.Algorithm).FirstOrDefault().ToString();
        //    //}
        //    //return algorithm;
        //}

        //private void AgentComboBox_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    AgentComboBox.ItemsSource = GetAgents();
        //}
        //private void AgentComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //}

        //private void ClienNameTxtBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    string result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ClienNameTxtBox.Text);
        //    ClienNameTxtBox.Text = result;
        //    ClienNameTxtBox.SelectionStart = ClienNameTxtBox.Text.Length;
        //}

        private void ClienNameTxtBox_KeyDown(object sender, KeyEventArgs e)
        {








            //string result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ClienNameTxtBox.Text);
            //ClienNameTxtBox.Text = result;
            //ClienNameTxtBox.SelectionStart = ClienNameTxtBox.Text.Length;
        }
        private void PathToRouter_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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
            //if (ConnectionName ))
            //    return "Задайте имя";

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
