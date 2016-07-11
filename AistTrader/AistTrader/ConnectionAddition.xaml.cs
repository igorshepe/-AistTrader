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
using Common.Params;
using Ecng.Common;

namespace AistTrader
{
    public partial class ConnectionAddition : IDataErrorInfo
    {
        #region Fields
        private int EditIndex { get; set; }
        private string _connectionName;
        private string _code;
        private string _providerPath;
        private string _selectedPath;
        private bool editMode;
        private string _originPath;
        private Connection connectionToEdit;
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
        public ConnectionAddition()
        {
            IsAdditionMode = true;
            DataContext = this;
            InitializeComponent();
            EditIndex = int.MinValue;
            AllPlazaDirectoriesComboBox.ItemsSource = GetAllPlazaDirectories();
            ConnectionNameTxtBox.Focus();
        }
        private List<string> GetAllPlazaDirectories()
        {
            List<string> list = new List<string>();
            var directories = Directory.EnumerateDirectories(@"C:\").Where(i => i.Contains("SpectraCGate")).ToList();
            if (IsAdditionMode)
            {
                if (!MainWindow.Instance.ConnectionsStorage.IsNull())
                {
                    var alreadyUsedPlazaRouters = MainWindow.Instance.ConnectionsStorage.Cast<Connection>().ToList();
                    var result = directories.Where(i => alreadyUsedPlazaRouters.All(i2 => i2.ConnectionParams.PlazaConnectionParams.Path != i)).ToList();
                    return result;
                }
            }
            list.AddRange(directories);
            return list; 
        }
        public ConnectionAddition(Connection account, int editIndex)
        {
            IsAdditionMode = false;
            InitializeComponent();
            InitFields(account);
            DataContext = this;
            EditIndex = editIndex;

        }
        private void InitFields(Connection connection)
        {
            connectionToEdit = connection;
            editMode = true;
            _connectionName = connection.DisplayName;
            _code = connection.ConnectionParams.Code;
            //Provider = account.ConnectionParams.PlazaConnectionParams.Type.ToString();
            //_providerPath= GetAllPlazaDirectories();
            AllPlazaDirectoriesComboBox.ItemsSource = GetAllPlazaDirectories();
            //AllPlazaDirectoriesComboBox.SelectedItem = account.Connection.ConnectionSettings.Path.ToString();
            _selectedPath = connection.ConnectionParams.PlazaConnectionParams.Path;
            _originPath = connection.ConnectionParams.PlazaConnectionParams.Path;
            IsDemoChkBox.IsChecked = connection.IsDemo;
            ConnectionNameTxtBox.Focus();
            
        }

        private bool IsFirtConnectionToBeSetAsDefault()
        {
            if (MainWindow.Instance.ConnectionsStorage.Count == 0)
                return true;
            else
                return false;
        }
        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            //    if (!File.Exists(AllPlazaDirectoriesComboBox.SelectedItem + @"\client_router.ini"))
            //    {
            //        //TODO: проверка на наличие роутера
            //        MessageBox.Show(this, @"В выбранной дериктории нет ini файла: {0}.".Put(AllPlazaDirectoriesComboBox.SelectedItem));
            //        return;
            //    }
            var terminalConnSettings = new PlazaConnectionParams(AllPlazaDirectoriesComboBox.SelectedItem.ToString());
            var connParams = new ConnectionParams(ConnectionNameTxtBox.Text, ClienCodeTxtBox.Text, terminalConnSettings, false, IsFirtConnectionToBeSetAsDefault());
            if (editMode)
            {
                var item = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.Id == connectionToEdit.Id);
                item.DisplayName = ConnectionNameTxtBox.Text;
                item.ConnectionParams = connParams;
                MainWindow.Instance.AddNewAgentConnection(connectionToEdit, EditIndex);
                editMode = false; //todo убрать
                connectionToEdit = null;
            }
            else
            {
                MainWindow.Instance.AddNewAgentConnection(new Connection(ConnectionNameTxtBox.Text, connParams, IsDemoChkBox.IsChecked.Value), EditIndex);
                if (MainWindow.Instance.ProviderCollectionView.Count == 1)
                    MainWindow.Instance.DefaultConnectionStatusBarText = "Default: " + ConnectionNameTxtBox.Text;
            }

            Close();
        }
        private void ConnectionNameTxtBox_KeyDown(object sender, KeyEventArgs e)
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
        private void ConnectionNameTxtBox_OnKeyUp(object sender, KeyEventArgs e)
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
                return "Enter name";
            else if (this.ConnectionName.Length < 5)
                return "Name should be 5 or more simbols long.";
            else if (NameAlredyInUse)
                return "This name already in use";
            else
                return String.Empty;
        }
        private string ValidateCode()
        {
            if (String.IsNullOrEmpty(this.Code))
                return "Enter code";
            else if (this.Code.Length < 2)
                return "Code should be 2 or more simbols long";
            else
                return String.Empty;
        }
        //private string ValidateProvider()
        //{
        //    //if (ConnectionTypeComboBox.SelectedItem == null)
        //    //{
        //    //    return "Не выбран поставщик";
        //    //}
        //    //return String.Empty;
        //}
        private string ValidateProviderPath()
        {
            if (AllPlazaDirectoriesComboBox.SelectedItem == null)
            {
                return "Select path to router";
            }
            if (AllPlazaDirectoriesComboBox != null)
            {
                if (!File.Exists(AllPlazaDirectoriesComboBox.SelectedItem + @"\client_router.ini"))
                {
                    return  string.Format("Selected path does not contain an ini fail: {0}".Put(AllPlazaDirectoriesComboBox.SelectedItem)) ;
                }    
            }
            if (AllPlazaDirectoriesComboBox.ItemsSource != null)
            {
                if (_originPath != AllPlazaDirectoriesComboBox.SelectedItem.ToString()) 
                {
                    var item = MainWindow.Instance.ConnectionsStorage.Any(i => i.ConnectionParams.PlazaConnectionParams.Path == AllPlazaDirectoriesComboBox.SelectedItem);
                    if (item)
                    {
                        return "Selected router already in use!";
                    }
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
        private void ConnectionNameTxtBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsAdditionMode)
            {
                var text = ConnectionNameTxtBox.Text;
                //todo: проработать логику
                //if (Settings.Default.AgentConnection != null)
                //    NameAlredyInUse = Settings.Default.AgentConnection.Cast<Connection>().Any(i => i.Name == text);    
            }
        }
    }
}
