using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using Ecng.Xaml;
using IniParser;
using NLog;
using StockSharp.BusinessEntities;
using StockSharp.Localization;
using StockSharp.Messages;
using StockSharp.Plaza;
using Strategies.Strategies;
using ToggleSwitch;

namespace AistTrader
{
    public partial class MainWindow
    {
        public List<Security> SecuritiesList = new List<Security>();
        public List<Portfolio> PortfoliosList = new List<Portfolio>();
        public PlazaTrader Trader = new PlazaTrader();
        public bool IsProviderSettingsLoaded;
        public  AistTraderConnnectionManager ConnectionManager;
        private void ProviderStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (ProviderStorage.Count == 2)
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
        public void AddNewAgentConnection(AgentConnection settings, int editIndex)
        {
            if (editIndex >= 0 && editIndex < ProviderStorage.Count)
                ProviderStorage[editIndex] = settings;
            else
                ProviderStorage.Add(settings);
            SaveProviderSettings();
            UpdateProviderListView();
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
                Logger.Log(LogLevel.Error, e.Message);
                Logger.Log(LogLevel.Error, e.InnerException.Message);
                if (e.InnerException.Message == "Root element is missing.")
                   File.WriteAllText("ProviderSettings.xml", string.Empty);
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
            //if ((sender as HorizontalToggleSwitch).IsChecked)
            //{
            //    //ON
            //    var item = (sender as FrameworkElement).DataContext;
            //    ProviderListView.SelectedItems.Clear();
            //    ProviderListView.SelectedItems.Add(item);
            //    ConnectAccount(item as AgentConnection);
            //}
            //else
            //{
            //    //OFF
            //    if (Trader != null && Trader.ConnectionState == ConnectionStates.Connected)
            //    {
            //        Trader.Disconnect();
            //        //set to null all collectionzzzZZzzz
            //        SecuritiesList.Clear();
            //        PortfoliosList.Clear();
            //    }
            //    var item = (sender as FrameworkElement).DataContext;
            //    var rowItem = ProviderStorage.FirstOrDefault(i => i == item);
            //    rowItem.Connection.IsActive = false;
            //}
        }

        public void UpdateProviderListView()
        {
            ProviderListView.ItemsSource = ProviderStorage;
            ProviderCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(ProviderListView.ItemsSource);
            ProviderCollectionView.Refresh();
            //ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
            //view.Refresh();
        }
        public void UpdateProviderGridListView(AgentConnection agentConnection)
        {
            var item = ProviderStorage.FirstOrDefault(i => i.Name == agentConnection.Name);
            item.Connection.VariationMargin = item.Connection.Accounts.FirstOrDefault().VariationMargin;
            item.Connection.Funds = item.Connection.Accounts.FirstOrDefault().CurrentValue;
            item.Connection.NetValue= item.Connection.Accounts.FirstOrDefault().CurrentPrice;


            ProviderListView.ItemsSource = ProviderStorage;
            ProviderCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(ProviderListView.ItemsSource);
            ProviderCollectionView.Refresh();
        }

        public void ConnectAccount(AgentConnection agent)
        {
            string ipEndPoint = "";
            if (agent.Connection.ConnectionSettings.IpEndPoint == null)
            {
                try
                {
                    Logger.Info("Trying to get the ip,port of plaza connection -{0}...",agent.Name.ToString());
                    ipEndPoint = GetPlazaConnectionIpPort(agent.Connection.ConnectionSettings.Path);
                    Logger.Info("IP and port of -{0} connection were successfully acquired", agent.Name.ToString());
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e.Message);
                    Logger.Log(LogLevel.Error, e.InnerException.Message);
                }
                var item = ProviderStorage.Cast<AgentConnection>().Where(i => i.Connection.ConnectionSettings.Path == agent.Connection.ConnectionSettings.Path)
                        .Select(i => i).FirstOrDefault();
                item.Connection.ConnectionSettings.IpEndPoint = ipEndPoint;
            }
            else
                ipEndPoint = agent.Connection.ConnectionSettings.IpEndPoint;
            var connection = new AistTraderConnnectionWrapper(agent.Name) {Address = ipEndPoint.To<IPEndPoint>(), IsCGate = true};


            //Trader = new PlazaTrader();
            //Trader.Address = ipEndPoint.To<IPEndPoint>();
            //Trader.IsCGate = true;
            //Trader.Name = "пох";

            //Trader.Connect();

            //Trader.NewSecurities += securities =>
            //{
            //    this.GuiAsync(() => agent.Connection.Tools.AddRange(securities))/* PortfoliosList.AddRange(portfolios))*/;
            //    //this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ MainWindow.Instance.AgentPortfolioStorage.(portfolios));
            //};
            //Trader.Connected += () =>
            //{
            //    //this.GuiAsync(() => agent.Connection.IsConnected = true);
            //    //this.GuiAsync(() => agent.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Connected);
            //    this.GuiAsync(UpdateProviderListView);
            //};
            //Trader.NewPortfolios += portfolios =>
            //{
            //    this.GuiAsync(() => agent.Connection.Accounts.AddRange(portfolios))/* PortfoliosList.AddRange(portfolios))*/;

            //    //this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ MainWindow.Instance.AgentPortfolioStorage.(portfolios));
            //};
            //Trader.Connect();

            //TODO: посмотри примеры того как идет динамический апдейт, а потом уже подписывай события на то что будет апдейтится

            agent.Connection.Accounts = new List<Portfolio>();
            agent.Connection.Tools = new List<Security>();

            agent.Connection.IsRegistredConnection = true;
            connection.NewPortfolios += portfolios =>
            {
                this.GuiAsync(() => agent.Connection.Accounts.AddRange(portfolios))/* PortfoliosList.AddRange(portfolios))*/;
                //this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ MainWindow.Instance.AgentPortfolioStorage.(portfolios));
                this.GuiAsync(() => UpdateProviderGridListView(agent));
                this.GuiAsync(() => Logger.Info("Portfolios were loaded"));
                TimeHelper.SyncMarketTime();
            };
            connection.NewSecurities += securities =>
            {
                //this.GuiAsync(() => agent.Connection.Tools.AddRange(securities))/* PortfoliosList.AddRange(portfolios))*/;
                //todo: поменять..уточниить про кол-во инструментов и особенностях получения
                this.GuiAsync(() =>
                {
                    agent.Connection.Tools.AddRange(securities)/* PortfoliosList.AddRange(portfolios))*/;
                    if (agent.Connection.Tools.Count > 1144)
                    {
                        Logger.Info("Securities were loaded");
                    }
                });
                //this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ MainWindow.Instance.AgentPortfolioStorage.(portfolios));
            };
            connection.Connected += () =>
            {
                this.GuiAsync(() => agent.Connection.IsConnected = true);
                this.GuiAsync(() => agent.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Connected);
                this.GuiAsync(() => UpdateProviderListView());
                this.GuiAsync(() => Logger.Info("Connection - {0} is active now", connection.Name));
            };
            connection.Disconnected += () =>
            {
                this.GuiAsync(() => agent.Connection.IsConnected = false);
                this.GuiAsync(() => agent.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Disconnected);
                this.GuiAsync(() => UpdateProviderListView());
                this.GuiAsync(() => Logger.Info("Connection - {0} is not active now", connection.Name));
            };
            //TODO: Добавить все эвенты по аналогии с портфелями
            //нужна ли динамика в отображениии данных, которые должны быть в табе соединений?
            connection.Connect();
            #region Trash

            //только в боевоей версии
            //Trader.CGateKey = "C99ElZcac2yZzSC9xSYqyaq8xXAnNrW";


            //Trader.ReConnectionSettings.AttemptCount = -1;
            ////what it is?
            //Trader.Restored += () => this.GuiAsync(() => MessageBox.Show(this, LocalizedStrings.Str2958));

            //// подписываемся на событие успешного соединения
            //Trader.Connected += () =>
            //{
            //    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Connected, agent));
            //    var x = Trader.CurrentTime;
            //};

            //// подписываемся на событие разрыва соединения
            //Trader.ConnectionError += error => this.GuiAsync(() =>
            //{
            //    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Disconnected, agent));
            //});

            //// подписываемся на событие успешного отключения
            ////Trader.Disconnected += () => this.GuiAsync(() => ChangeConnectStatus(false));


            //Trader.NewSecurities += securities =>
            //{
            //    this.GuiAsync(() => SecuritiesList.AddRange(securities) /*agent.AgentAccount.Tools.AddRange(securities)*/   );
            //};


            //Trader.NewPortfolios += portfolios =>
            //{
            //    this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ PortfoliosList.AddRange(portfolios));
            //    //this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ MainWindow.Instance.AgentPortfolioStorage.(portfolios));
            //};

            ////Trader.Connected += () =>
            ////{

            ////    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Connected, agent));
            ////};


            //Trader.Disconnected += () =>
            //{
            //    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Disconnected, agent));
            //    Trader.Dispose();
            //};
            //Trader.ConnectionError += error => this.GuiAsync(() =>
            //{
            //    var x = error;
            //    ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.ConnectionError, agent);
            //});


            //Trader.Connect();



            ////_PlazaTrader = new PlazaTrader
            ////{
            ////    UseLocalProtocol = true,
            ////    Address = ipEndPoint.To<IPEndPoint>()
            ////};

            ////_PlazaTrader.NewPortfolios += portfolios =>
            ////{
            ////    this.GuiAsync(() => /*agent.AgentAccount.Accounts.AddRange(portfolios)*/ PortfoliosList.AddRange(portfolios));
            ////};

            ////_PlazaTrader.NewSecurities += securities =>
            ////{
            ////    //this.GuiAsync(() => SecuritiesList.AddRange(securities) /*agent.AgentAccount.Tools.AddRange(securities)*/   );
            ////};

            ////_PlazaTrader.ReConnectionSettings.Interval = TimeSpan.FromSeconds(5);
            //////_PlazaTrader.ReConnectionSettings.Connectio += () => this.GuiAsync(() => MessageBox.Show(this, "Соединение восстановлено."));
            ////_PlazaTrader.Connected += () =>
            ////{
            ////  //  _PlazaTrader.StartExport();
            ////    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Connected, agent));
            ////};
            ////_PlazaTrader.Disconnected += () =>
            ////{
            ////    this.GuiAsync(() => ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.Disconnected, agent));
            ////    _PlazaTrader.Dispose();
            ////};
            ////_PlazaTrader.ConnectionError += error => this.GuiAsync(() =>
            ////{
            ////    ConnectionStatus(ConnectionsSettings.AgentConnectionStatus.ConnectionError, agent);
            ////    _PlazaTrader.Dispose();
            ////});
            ////_PlazaTrader.Connect();


            #endregion
            ConnectionManager.Add(connection);
        }

        public static string GetPlazaConnectionIpPort(string plazaPath)
        {
            //TODO: исключить статику если потребуются
            plazaPath = plazaPath + @"\client_router.ini";
            FileIniDataParser file = new FileIniDataParser();
            IniData data = file.LoadFile(plazaPath);
            var port = data["P2MQRouter"]["port"];
            var address = IPAddress.Loopback.ToString() +":"+ port;
            // у нас есть два исключения когда роутер и тслаб на разных серверах, в таком случае соответственно не локалхост
            return address;
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
            if ((sender as Button).Content.ToString() == "Connect")
            {
                //ON
                var item = (sender as FrameworkElement).DataContext;
                ProviderListView.SelectedItems.Clear();
                ProviderListView.SelectedItems.Add(item);
                var rowItem = Instance.ProviderStorage.FirstOrDefault(i => i == item);
                int index = ConnectionManager.Connections.FindIndex(i => i.Name == rowItem.Name);
                rowItem.Connection.Command = OperationCommand.Disconnect;
                if (rowItem.Connection.IsRegistredConnection)
                {
                    ConnectionManager.Connections[index].Connect();
                }
                else
                {
                    ConnectAccount(item as AgentConnection);
                }
                UpdateProviderListView();
                //ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
                //view.Refresh();
            }
            else
            {
                //OFF
                //if (Trader != null /*&& Trader.ConnectionState == ConnectionStates.Connected*/)
                //{
                //    Trader.Disconnect();
                //    //set to null all collectionzzzZZzzz
                //    SecuritiesList.Clear();
                //    PortfoliosList.Clear();
                //}
                //ConnectionManager.Connections[0].Disconnect();
                var item = (sender as FrameworkElement).DataContext;
                var rowItem = ProviderStorage.FirstOrDefault(i => i == item);
                int index = ConnectionManager.Connections.FindIndex(i => i.Name == rowItem.Name);
                ConnectionManager.Connections[index].Disconnect();
                rowItem.Connection.Command = OperationCommand.Connect;
                //rowItem.Connection.ConnectionStatus = ConnectionsSettings.AgentConnectionStatus.Disconnected;

                UpdateProviderListView();
                //ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
                //view.Refresh();
            }
        }
        private void ProviderListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsProviderSettingsLoaded & (File.Exists("ProviderSettings.xml")) & ProviderStorage.Count == 0)
                InitiateProviderSettings();
        }


    }

    #region Aist Trader Connection Manager
    public class AistTraderConnnectionWrapper : PlazaTrader
    {
        public AistTraderConnnectionWrapper(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
        public new string Name { get; set; }
    }
    public class AistTraderConnnectionManager : IList<AistTraderConnnectionWrapper>, IDisposable
    {
        public  List<AistTraderConnnectionWrapper> Connections = new List<AistTraderConnnectionWrapper>();

        public IEnumerator<AistTraderConnnectionWrapper> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(AistTraderConnnectionWrapper item)
        {
            Connections.Add(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(AistTraderConnnectionWrapper item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(AistTraderConnnectionWrapper[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(AistTraderConnnectionWrapper item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; set; }
        public bool IsReadOnly { get; set; }
        public int IndexOf(AistTraderConnnectionWrapper item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, AistTraderConnnectionWrapper item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public AistTraderConnnectionWrapper this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
