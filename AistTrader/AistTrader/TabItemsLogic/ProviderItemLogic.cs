using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Data;
using AistTrader.Properties;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using Ecng.Xaml;
using IniParser;
using StockSharp.BusinessEntities;
using StockSharp.Localization;
using StockSharp.Messages;
using StockSharp.Plaza;
using ToggleSwitch;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace AistTrader
{
    public partial class MainWindow
    {
        public ObservableCollection<AgentConnection> ProviderStorage { get; private set; }
        public readonly PlazaTrader Trader = new PlazaTrader();
        const string Localhost = "127.0.0.1:4001";
        public List<Security> SecuritiesList = new List<Security>();
        public List<Portfolio> PortfoliosList = new List<Portfolio>();
        public bool IsProviderSettingsLoaded;


        private void LoadProviderTabItemData()
        {
        }

        private void ProviderStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (ProviderStorage.Count == 1)
                AddConnectionBtn.IsEnabled = false;
            else
                AddConnectionBtn.IsEnabled = true;
        }

        //private void ChangeConnectionNotice(PlazaTrader trader)
        //{
        //    //TODO: тогда когда есть персональные соединения
        //    //var rowItem = AgentAccountStorage.FirstOrDefault(a => a.AgentAccount.Login == trader.Login);
        //    //rowItem.AgentAccount.IsActive = false;
        //}
        public static void ConnectionStatus(ConnectionsSettings.AgentConnectionStatus agentConnStatus, AgentConnection item)
        {
            if (agentConnStatus == ConnectionsSettings.AgentConnectionStatus.Connected)
            {
                item.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Connected;
            }
            if (agentConnStatus == ConnectionsSettings.AgentConnectionStatus.Disconnected)
            {
                item.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Disconnected;
            }
            ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
            view.Refresh();


            //var rowItem = Instance.ProviderStorage.FirstOrDefault(i => i == item);
            //rowItem.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Authentication;
            //TODO: при выводе сообщений добавлять инфу о том какое именно соеднение..
        }
        //private void OrdersFailed(IEnumerable<OrderFail> fails)
        //{
        //    this.GuiAsync(() =>
        //    {
        //        foreach (var fail in fails)
        //            MessageBox.Show(this, fail.Error.ToString(), "Ошибка регистрации заявки");
        //    });
        //}
        public void AddNewAgentConnection(AgentConnection settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < ProviderStorage.Count)
                ProviderStorage[editIndex] = settings;
            else
                ProviderStorage.Add(settings);
            SaveProviderSettings();
        }
        private void SaveProviderSettings()
        {
            List<AgentConnection> obj = ProviderStorage.Select(a => a).ToList();
            var fStream = new FileStream("ProviderSettings.xml", FileMode.Create, FileAccess.Write, FileShare.None);
            var xmlSerializer = new XmlSerializer(typeof(List<AgentConnection>), new Type[] { typeof(AgentConnection) });
            xmlSerializer.Serialize(fStream, obj);
            fStream.Close();
        }
        private void InitiateProviderSettings()
        {
            StreamReader sr = new StreamReader("ProviderSettings.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<AgentConnection>), new Type[] { typeof(AgentConnection) });
                var connections = (List<AgentConnection>)xmlSerializer.Deserialize(sr);
                sr.Close();
                if (connections == null) return;
                foreach (var rs in connections)
                {
                    ProviderStorage.Add(rs);
                }
                ProviderListView.ItemsSource = ProviderStorage;
                IsProviderSettingsLoaded = true;
            }
            catch (Exception e)
            {
                IsProviderSettingsLoaded = false;
                sr.Close();
                if (e.InnerException.Message == "Root element is missing.")
                   System.IO.File.WriteAllText("ProviderSettings.xml", string.Empty);
            }
        }
        private void AddAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new AgentConnectionAddition();
            form.ShowDialog();
            form = null;
            //Todo:save settings
        }

        private void DelAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ProviderListView.SelectedItems.Cast<AgentConnection>().ToList())
            {
                if (PortfolioListView.Items.Cast<AgentPortfolio>().Any(i => i.Connection.Name == item.Name))
                {
                    MessageBox.Show(this, @"На данном соединении завязан портфель, удаление невозможно!");
                    return;
                }
                ProviderStorage.Remove(item);
                SaveProviderSettings();
            }
            //foreach (var item in ProviderListView.SelectedItems.Cast<AgentPortfolio>().ToList())
            //{
            //    AgentPortfolioStorage.Remove(item);
            //}
        }
        private void EditAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = ProviderListView.SelectedItems.Cast<AgentConnection>().ToList();
            //TODO: переделать
            foreach (var connectionEditWindow in from agentSettings in listToEdit
                                                 let index = ProviderStorage.IndexOf(agentSettings)
                                                 where index != -1
                                                 select new AgentConnectionAddition(agentSettings, index))
            {
                connectionEditWindow.Title = "Аист Трейдер - Редактировать подключение";
                connectionEditWindow.ShowDialog();
                connectionEditWindow.Close();
            }


        }
        private void AgentManagerDetails_Loaded(object sender, RoutedEventArgs e)
        {
            //ChkBoxSelectAll.IsChecked= AgentAccountStorage.All(x => x.AgentAccount.IsEnabled);
        }
        private void ConnectionStateSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as HorizontalToggleSwitch).IsChecked)
            {
                //ON
                var item = (sender as FrameworkElement).DataContext;
                ProviderListView.SelectedItems.Clear();
                ProviderListView.SelectedItems.Add(item);
                ConnectAccount(item as AgentConnection);
            }
            else
            {
                //OFF
                if (Trader != null && Trader.ConnectionState == ConnectionStates.Connected)
                {
                    Trader.Disconnect();
                    //set to null all collectionzzzZZzzz
                    SecuritiesList.Clear();
                    PortfoliosList.Clear();
                }
                
                var item = (sender as FrameworkElement).DataContext;
                var rowItem = ProviderStorage.FirstOrDefault(i => i == item);
                rowItem.Connection.IsActive = false;
            }
        }

        public void ConnectAccount(AgentConnection agent)
        {
            string ipEndPoint = "";
            if (agent.Connection.ConnectionSettings.IpEndPoint == null)
            {
                ipEndPoint = ReadPlazaPersonalSettings(agent.Connection.ConnectionSettings.Path);
                var item = ProviderStorage.Cast<AgentConnection>().Where(i => i.Connection.ConnectionSettings.Path == agent.Connection.ConnectionSettings.Path)
                        .Select(i => i).FirstOrDefault();
                item.Connection.ConnectionSettings.IpEndPoint = ipEndPoint;
            }
            else
                ipEndPoint = agent.Connection.ConnectionSettings.IpEndPoint;

            Trader.AppName = "TestCGateConnection";
            Trader.Address = ipEndPoint.To<IPEndPoint>();
            Trader.IsCGate = true;
            //            Trader.CGateKey = null;

            //только в боевоей версии
            //Trader.CGateKey = "C99ElZcac2yZzSC9xSYqyaq8xXAnNrW";


            Trader.ReConnectionSettings.AttemptCount = -1;
            //what it is?
            Trader.Restored += () => this.GuiAsync(() => MessageBox.Show(this, LocalizedStrings.Str2958));

            // подписываемся на событие успешного соединения
            Trader.Connected += () =>
            {
                this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Connected, agent));
            };

            // подписываемся на событие разрыва соединения
            Trader.ConnectionError += error => this.GuiAsync(() =>
            {
                this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Disconnected, agent));
            });

            // подписываемся на событие успешного отключения
            //Trader.Disconnected += () => this.GuiAsync(() => ChangeConnectStatus(false));


            Trader.NewSecurities += securities =>
            {
                this.GuiAsync(() => SecuritiesList.AddRange(securities) /*agent.AgentAccount.Tools.AddRange(securities)*/   );
            };


            Trader.NewPortfolios += portfolios =>
            {
                this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ PortfoliosList.AddRange(portfolios));
            };

            //Trader.Connected += () =>
            //{

            //    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Connected, agent));
            //};


            Trader.Disconnected += () =>
            {
                this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Disconnected, agent));
                Trader.Dispose();
            };
            Trader.ConnectionError += error => this.GuiAsync(() =>
            {
                var x = error;
                ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.ConnectionError, agent);
            });


            Trader.Connect();



            //_PlazaTrader = new PlazaTrader
            //{
            //    UseLocalProtocol = true,
            //    Address = ipEndPoint.To<IPEndPoint>()
            //};

            //_PlazaTrader.NewPortfolios += portfolios =>
            //{
            //    this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ PortfoliosList.AddRange(portfolios));
            //};

            //_PlazaTrader.NewSecurities += securities =>
            //{
            //    //this.GuiAsync(() => SecuritiesList.AddRange(securities) /*agent.AgentAccount.Tools.AddRange(securities)*/   );
            //};

            //_PlazaTrader.ReConnectionSettings.Interval = TimeSpan.FromSeconds(5);
            ////_PlazaTrader.ReConnectionSettings.Connectio += () => this.GuiAsync(() => MessageBox.Show(this, "Соединение восстановлено."));
            //_PlazaTrader.Connected += () =>
            //{
            //  //  _PlazaTrader.StartExport();
            //    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Connected, agent));
            //};
            //_PlazaTrader.Disconnected += () =>
            //{
            //    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Disconnected, agent));
            //    _PlazaTrader.Dispose();
            //};
            //_PlazaTrader.ConnectionError += error => this.GuiAsync(() =>
            //{
            //    ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.ConnectionError, agent);
            //    _PlazaTrader.Dispose();
            //});
            //_PlazaTrader.Connect();
        }
        public static string ReadPlazaPersonalSettings(string plazaPath)
        {
            //TODO: исключить статику если потребуются
            plazaPath = plazaPath + @"\client_router.ini";
            FileIniDataParser file = new FileIniDataParser();

            //IniData data = file.LoadFile(plazaPath);
            //var port = data["P2MQRouter"]["port"];
            //            var address =  Localhost + port;
            //TODO: уточнить по IP, тоже получить в динамике
            // у нас есть два исключения когда роутер и тслаб на разных серверах, в таком случае соответственно не локалхост
            return Localhost;
        }

        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void OperationBtnClick(object sender, RoutedEventArgs e)
        {

            if ((sender as System.Windows.Controls.Button).Content.ToString() == "Connect")
            {
                //ON
                var item = (sender as FrameworkElement).DataContext;
                ProviderListView.SelectedItems.Clear();
                ProviderListView.SelectedItems.Add(item);


                var rowItem = Instance.ProviderStorage.FirstOrDefault(i => i == item);
                //на время попытки подключения ставим Disconnect
                rowItem.Connection.Command = OperationCommand.Disconnect;
                rowItem.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Authentication;
                ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
                view.Refresh();

                ConnectAccount(item as AgentConnection);

            }
            else
            {
                //OFF
                if (Trader != null /*&& Trader.ConnectionState == ConnectionStates.Connected*/)
                {
                    Trader.Disconnect();
                    //set to null all collectionzzzZZzzz
                    SecuritiesList.Clear();
                    PortfoliosList.Clear();
                }

                var item = (sender as FrameworkElement).DataContext;
                var rowItem = ProviderStorage.FirstOrDefault(i => i == item);
                rowItem.Connection.Command = OperationCommand.Connect;
                rowItem.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Disconnected;
                ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
                view.Refresh();
            }
        }

        private void ProviderListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsProviderSettingsLoaded && (File.Exists("ProviderSettings.xml")))
                InitiateProviderSettings();
            //if (AgentsStorage.Count > 0)
            //    EditSingleOrGroupItemBtn.IsEnabled = true;
            //else
            //    EditSingleOrGroupItemBtn.IsEnabled = false;
        }


    }
}
