﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using Ecng.Xaml;
using NLog;
using StockSharp.BusinessEntities;
using StockSharp.Plaza;
using System.Threading.Tasks;
using MahApps.Metro.Controls;

namespace AistTrader
{
    public partial class MainWindow
    {
        #region Fields
        public bool IsProviderSettingsLoaded;
        private bool currentConnectionIsConnected = true;
        public  AistTraderConnnectionManager ConnectionManager;

        #endregion
        private void ProviderStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
        }

        public void AddNewAgentConnection(Connection connection, int editIndex)
        {
            if (editIndex >= 0 && editIndex < ConnectionsStorage.Count)
            {
                ConnectionsStorage[editIndex] = connection;
            }
            else
            {
                ConnectionsStorage.Add(connection);
            }

            SaveProviderItems();
            UpdateProviderListView();
        }

        private void SaveProviderItems()
        {
            try
            {
                List<Connection> obj = ConnectionsStorage.Select(a => a).ToList();
                var tList = new List<Type>();
                tList.Add(typeof(Common.Entities.Portfolio));
                tList.Add(typeof(System.TimeZoneInfo));
                tList.Add(typeof(TimeZoneInfo.AdjustmentRule[]));
                tList.Add(typeof(TimeZoneInfo.AdjustmentRule));
                tList.Add(typeof(TimeZoneInfo.TransitionTime));
                tList.Add(typeof(System.DayOfWeek));
                using (var fStream = new FileStream("Connections.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var xmlSerializer = new DataContractSerializer(typeof(List<Connection>), tList);
                    xmlSerializer.WriteObject(fStream, obj);
                    fStream.Close();
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => Logger.Log(LogLevel.Error, ex.Message));
            }
        }

        private void InitiateProviderItems()
        {
            NtpMoexSync();
            using (FileStream sr = new FileStream("Connections.xml", FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var tList = new List<Type>();
                    tList.Add(typeof(Common.Entities.Portfolio));
                    tList.Add(typeof(System.TimeZoneInfo));
                    tList.Add(typeof(TimeZoneInfo.AdjustmentRule[]));
                    tList.Add(typeof(TimeZoneInfo.AdjustmentRule));
                    tList.Add(typeof(TimeZoneInfo.TransitionTime));
                    tList.Add(typeof(System.DayOfWeek));
                    var xmlSerializer = new DataContractSerializer(typeof(List<Connection>), tList);
                    var connections = (List<Connection>)xmlSerializer.ReadObject(sr);
                    sr.Close();
                    if (connections == null) return;
                    foreach (var rs in connections)
                    {
                        ConnectionsStorage.Add(rs);
                    }
                    ProviderListView.ItemsSource = ConnectionsStorage;
                    IsProviderSettingsLoaded = true;
                }
                catch (Exception e)
                {
                    IsProviderSettingsLoaded = false;
                    sr.Close();
                    Task.Run(() => Logger.Log(LogLevel.Error, e.Message));
                    Task.Run(() => Logger.Log(LogLevel.Error, e.InnerException.Message));
                    if (e.InnerException.Message == "Root element is missing.")
                    {
                        File.WriteAllText("Connections.xml", string.Empty);
                    }
                }
            }

            var firstOrDefault = ConnectionsStorage.FirstOrDefault(i => i.ConnectionParams.IsDefaulConnection);
            if (firstOrDefault != null)
            {
                DefaultConnectionStatusBarText = "Default: " + firstOrDefault.DisplayName;
            }
            else
            {
                DefaultConnectionStatusBarText = "Default connection is not set";
            }
        }

        private void AddAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new ConnectionAddition();
            form.ShowDialog();
            form = null;
        }

        private async void DelAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            //todo: оптимизировать запросы, выборки
            var selectedItem = ProviderListView.SelectedItem as Connection;
            if (selectedItem.ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Connected)
            {
                MessageBox.Show(this, @"Can not be deleted, connection is active"); 
                return;
            }
            MessageBoxResult result = MessageBox.Show("Connection \"{0}\" will be deleted! You sure?".Put(ProviderListView.SelectedItem),"Delete connection", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in ProviderListView.SelectedItems.Cast<Connection>().ToList())
                {
                    if (PortfolioListView.Items.Cast<Common.Entities.Portfolio>().Any(i => i.Connection.DisplayName == item.DisplayName))
                    {
                        MessageBox.Show(this, @"Can not be deleted, used in portfolio!");
                        return;
                    }

                    var connection = ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == item.DisplayName);
                    if (connection != null)
                    {
                        connection.Disconnect();
                        connection.Dispose();
                        ConnectionManager.Connections.Remove(connection);
                        Logger.Info("Connection \"{0}\" has been deleted", connection.ConnectionName);
                    }
                    ConnectionsStorage.Remove(item);
                    if (ConnectionsStorage.Count == 0)
                    {
                        DefaultConnectionStatusBarText = "Default connection is not set";
                    }
                    else
                    {
                        var value = ConnectionsStorage.First();
                        value.ConnectionParams.IsDefaulConnection = true;
                        DefaultConnectionStatusBarText = "Default: " + value.DisplayName;
                        ConnectionStatusTextBlock.Text = value.ConnectionParams.ConnectionState.ToString();
                    }
                    SaveProviderItems();
                }
            }
        }

        private void EditAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = ProviderListView.SelectedItems.Cast<Connection>().ToList();
            if (listToEdit[0].ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Connected)
            {
                //todo: заменить на неактивную кнопку редактирования, убрать предупреждения
                MessageBox.Show(this, @"Active connection can not be edited!");
                return;
            }
            //TODO: переделать
            foreach (var connectionEditWindow in from agentSettings in listToEdit
                                                 let index = ConnectionsStorage.IndexOf(agentSettings)
                                                 where index != -1
                                                 select new ConnectionAddition(agentSettings, index))
            {
                connectionEditWindow.Title = "Aist Trader - Edit connection";
                connectionEditWindow.ShowDialog();
                connectionEditWindow.Close();
            }
        }

        private void ConnectionStateSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            ResetStarted();

            if ((bool)(sender as ToggleSwitchButton).IsChecked)
            {
                if (!currentConnectionIsConnected) { return; }

                //ON
                var item = (sender as FrameworkElement).DataContext;
                var rowItem = Instance.ConnectionsStorage.FirstOrDefault(i => i == item);
                int index = ConnectionManager.Connections.FindIndex(i =>
                {
                    return rowItem != null && i.ConnectionName == rowItem.DisplayName;
                });
                rowItem.ConnectionParams.Command = OperationCommand.Disconnect;
                currentConnectionIsConnected = false;
                if (rowItem.ConnectionParams.IsRegistredConnection)
                {
                    Task.Factory.StartNew(() =>
                    {
                        ConnectionManager.Connections[index].Connect();
                        currentConnectionIsConnected = true;
                    });
                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        ConnectAccount(item as Connection);
                        currentConnectionIsConnected = true;
                    });
                }
            }
            else
            {
                //OFF
                var item = (sender as FrameworkElement).DataContext;
                var rowItem = ConnectionsStorage.FirstOrDefault(i => i == item);
                var con = ConnectionManager.Connections.FirstOrDefault(m => rowItem != null && m.ConnectionName == rowItem.ToString());
                int index = ConnectionManager.Connections.FindIndex(i =>
                {
                    return rowItem != null && i.ConnectionName == rowItem.DisplayName;
                });
                ConnectionManager.Connections[index].Disconnect();

                if (rowItem != null)
                {
                    rowItem.ConnectionParams.Command = OperationCommand.Connect;
                }
            }
        }

        public void UpdateProviderListView()
        {
            ProviderListView.ItemsSource = ConnectionsStorage;
            ProviderCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(ProviderListView.ItemsSource);
            ProviderCollectionView.Refresh();
        }

        public void UpdateProviderGridListView(Connection agentConnection)
        {
            var item = ConnectionsStorage.FirstOrDefault(i => i.DisplayName == agentConnection.DisplayName);
            var firstOrDefault = item.ConnectionParams.Accounts.FirstOrDefault();
            if (firstOrDefault != null)
            {
                item.ConnectionParams.VariationMargin = firstOrDefault.VariationMargin == null ? 0 : (decimal)firstOrDefault.VariationMargin;
            }
            var orDefault = item.ConnectionParams.Accounts.FirstOrDefault();
            if (orDefault != null)
            {
                item.ConnectionParams.Funds = orDefault.CurrentValue == null ? 0 : (decimal)orDefault.CurrentValue;
            }
            var portfolio = item.ConnectionParams.Accounts.FirstOrDefault();
            if (portfolio != null)
            {
                item.ConnectionParams.NetValue = portfolio.CurrentPrice == null ? 0 : (decimal)portfolio.CurrentPrice;
            }

            ProviderListView.ItemsSource = ConnectionsStorage;
            ProviderCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(ProviderListView.ItemsSource);
        }

        public void ConnectAccount(Connection conn)
        {
            string ipEndPoint = "";
            bool sLoaded = false;
            bool oLoaded = false;

            if (conn.ConnectionParams.PlazaConnectionParams.IpEndPoint == null)
            {
                try
                {
                    Task.Run(() => Logger.Info("Trying to get the ip,port of plaza connection -\"{0}\"...", conn.DisplayName.ToString()));
                    ipEndPoint = GetPlazaConnectionIpPort(conn.ConnectionParams.PlazaConnectionParams.Path);
                    Task.Run(() => Logger.Info("IP and port of -\"{0}\" connection were successfully acquired", conn.DisplayName.ToString()));
                }
                catch (Exception e)
                {
                    Task.Run(() => Logger.Log(LogLevel.Error, e.Message));
                    Task.Run(() => Logger.Log(LogLevel.Error, e.InnerException.Message));

                    return;

                }
                var item = ConnectionsStorage.Cast<Connection>().Where(i => i.ConnectionParams.PlazaConnectionParams.Path == conn.ConnectionParams.PlazaConnectionParams.Path)
                        .Select(i => i).FirstOrDefault();
                item.ConnectionParams.PlazaConnectionParams.IpEndPoint = ipEndPoint;
            }
            else
            {
                ipEndPoint = conn.ConnectionParams.PlazaConnectionParams.IpEndPoint;
            }
            var connection = new AistTraderConnnectionWrapper(conn.DisplayName) {Address = ipEndPoint.To<IPEndPoint>(), IsCGate = true, IsDemo = conn.IsDemo};

            //TODO: посмотри примеры того как идет динамический апдейт, а потом уже подписывай события на то что будет апдейтится
            conn.ConnectionParams.Accounts = new List<StockSharp.BusinessEntities.Portfolio>();
            conn.ConnectionParams.Tools = new List<Security>();
            conn.ConnectionParams.IsRegistredConnection = true;

            connection.NewPortfolios += portfolios =>
            {
                this.GuiAsync(() => _portfoliosWindow.PortfolioGrid.Portfolios.AddRange(portfolios)); //для тестов
                this.GuiAsync(() => conn.ConnectionParams.Accounts.AddRange(portfolios));
                this.GuiAsync(() => UpdateProviderGridListView(conn));
                this.GuiAsync(() => Logger.Info("Portfolios for connection \"{0}\" were loaded",connection.ConnectionName));
            };
            connection.NewSecurities += securities =>
            {

                if (conn.ConnectionParams.IsDefaulConnection)
                {
                    this.GuiAsync(() => _securitiesWindow.SecurityPicker.Securities.AddRange(securities)); //для тестов
                }

                this.GuiAsync(() =>
                {
                    conn.ConnectionParams.Tools.AddRange(securities);
                    if (conn.ConnectionParams.Tools.Count > 10 && !sLoaded)
                    {
                        sLoaded = true;
                        Task.Run(() => Logger.Info("Securities for connection \"{0}\" were loaded", connection.ConnectionName));
                    }
                });
            };

            connection.NewPositions += positions =>
            {
                _portfoliosWindow.PortfolioGrid.Positions.AddRange(positions); // Для тестов 
            };

            var ordersHistory = new List<Order>();
            connection.NewOrders += orders =>
            {
                _ordersWindow.OrderGrid.Orders.AddRange(orders); // Для тестов

                this.GuiAsync(() =>
                {
                    ordersHistory.AddRange(orders);
                    if (ordersHistory.Count > 1 && !oLoaded)
                    {
                        oLoaded = true;
                        Task.Run(() => Logger.Info("Orders for connection \"{0}\" were loaded", connection.ConnectionName));
                    }
                });
            };

            connection.NewMyTrades += trades =>
            {
                _myTradesWindow.TradeGrid.Trades.AddRange(trades); // Для тестов 
            };
            _securitiesWindow.SecurityPicker.MarketDataProvider = connection; // Для тестов 
            connection.PortfolioChanged += portfolio =>
            {
                var test = portfolio;
            };
            connection.Connected += () =>
            {
                this.GuiAsync(() => conn.ConnectionParams.IsConnected = true);
                this.GuiAsync(() => conn.ConnectionParams.ConnectionState = ConnectionParams.ConnectionStatus.Connected);
                this.GuiAsync(UpdateProviderListView);
                this.GuiAsync(() => Logger.Info("Connection - \"{0}\" is active now", connection.ConnectionName));
                if (conn.ConnectionParams.IsDefaulConnection)
                {
                    this.GuiAsync(() => Instance.ConnectionStatusTextBlock.Text = ConnectionParams.ConnectionStatus.Connected.ToString());
                }
            };
            connection.Disconnected += () =>
            {
                this.GuiAsync(() => conn.ConnectionParams.IsConnected = false);
                this.GuiAsync(() => conn.ConnectionParams.ConnectionState = ConnectionParams.ConnectionStatus.Disconnected);   
                this.GuiAsync(UpdateProviderListView);
                this.GuiAsync(() => Logger.Info("Connection - \"{0}\" is not active now", connection.ConnectionName));
                if (conn.ConnectionParams.IsDefaulConnection)
                {
                    this.GuiAsync(() => Instance.ConnectionStatusTextBlock.Text = ConnectionParams.ConnectionStatus.Disconnected.ToString());
                }
            };
            connection.ConnectionError += error =>
            {
                this.GuiAsync(() => Logger.Info("Connection error -\"{0}\", connection \"{1}\" is not active", error, connection.Name));
                //todo: коды ошибок в енам, подкидывать готовые уведомления на примере нерабочего роутера, подсвечивать в гриде цветом, если ошибка критическая
            };
            //TODO: Добавить все эвенты по аналогии с портфелями
            connection.Connect();
            ConnectionManager.Add(connection);
        }

        public static string GetPlazaConnectionIpPort(string plazaPath)
        {
            //TODO: исключить статику если потребуются
            plazaPath = plazaPath + @"\client_router.ini";
            string port = File.ReadAllText(plazaPath).Split('\r', '\n').First(st => st.StartsWith("port"));
            port = port.Substring(port.Length -4);
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

        private void ProviderListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsProviderSettingsLoaded & (File.Exists("Connections.xml")) & ConnectionsStorage.Count == 0)
            {
                InitiateProviderItems();
            }

            EditAgentConnectionBtn.IsEnabled = DelAgentConnectionBtn.IsEnabled = ProviderListView.Items.Count != 0;
        }

        private void ProviderListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProviderListView.Items.Count == 0)
            {
                EditAgentConnectionBtn.IsEnabled = false;
                DelAgentConnectionBtn.IsEnabled = false;
                DefaultConnectionStatusBarText = "Default connection is not set";
            }
            else
            {
                EditAgentConnectionBtn.IsEnabled = true;
                DelAgentConnectionBtn.IsEnabled = true;
            }
        }

        private void ConnectionsContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var conn in ConnectionsStorage)
            {
                conn.ConnectionParams.IsDefaulConnection = false;
            }
            var item = ProviderListView.SelectedItem as Connection;
            item.ConnectionParams.IsDefaulConnection = true;
            DefaultConnectionStatusBarText = "Default: " + item.DisplayName;
            Task.Run(() => Logger.Info("Connection - \"{0}\" is set to be default connection", item.DisplayName));
            if (item.ConnectionParams.IsConnected)
            {
                Instance.ConnectionStatusTextBlock.Text = ConnectionParams.ConnectionStatus.Connected.ToString();
            }
            else
            {
                Instance.ConnectionStatusTextBlock.Text = ConnectionParams.ConnectionStatus.Disconnected.ToString();
            }
            SaveProviderItems();
            UpdateProviderListView();
        }
    }

    #region Aist Trader Connection Manager
    public class AistTraderConnnectionWrapper : PlazaTrader
    {
        public AistTraderConnnectionWrapper(string name)
        {
            ConnectionName = name;
        }

        public string ConnectionName { get; set; }

        //((PlazaTrader)this).Timeout += null;
    }

    public class AistTraderConnnectionManager : IList<AistTraderConnnectionWrapper>, IDisposable
    {
        public  List<AistTraderConnnectionWrapper> Connections = new List<AistTraderConnnectionWrapper>();

        public IEnumerator<AistTraderConnnectionWrapper> GetEnumerator()
        {
            return Connections.GetEnumerator();
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
            Connections.Clear();
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
             return  Connections.Remove(item);
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
        }
    }

    #endregion
}
