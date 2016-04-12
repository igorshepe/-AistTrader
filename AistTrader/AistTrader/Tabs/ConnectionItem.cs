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
using Common.Params;
using Ecng.Common;
using Ecng.Xaml;
using IniParser;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using StockSharp.BusinessEntities;
using StockSharp.Plaza;
using System.Threading.Tasks;

namespace AistTrader
{
    public partial class MainWindow
    {
        #region Fields
        public bool IsProviderSettingsLoaded;
        public  AistTraderConnnectionManager ConnectionManager;
        #endregion
        private void ProviderStorageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if(ConnectionsStorage.Count == 2)
                AddConnectionBtn.IsEnabled = false;
            else
                AddConnectionBtn.IsEnabled = true;
        }
        public void AddNewAgentConnection(Connection connection, int editIndex)
        {
            if (editIndex >= 0 && editIndex < ConnectionsStorage.Count)
                ConnectionsStorage[editIndex] = connection;
            else
                ConnectionsStorage.Add(connection);
            SaveProviderItems();
            UpdateProviderListView();
        }
        private void SaveProviderItems()
        {
            List<Connection> obj = ConnectionsStorage.Select(a => a).ToList();
            var fStream = new FileStream("Connections.xml", FileMode.Create, FileAccess.Write, FileShare.None);
            var xmlSerializer = new XmlSerializer(typeof(List<Connection>), new Type[] { typeof(Connection) });
            xmlSerializer.Serialize(fStream, obj);
            fStream.Close();
        }
        private void InitiateProviderItems()
        {
            StreamReader sr = new StreamReader("Connections.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<Connection>), new Type[] { typeof(Connection) });
                var connections = (List<Connection>)xmlSerializer.Deserialize(sr);
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
                Logger.Log(LogLevel.Error, e.Message);
                Logger.Log(LogLevel.Error, e.InnerException.Message);
                if (e.InnerException.Message == "Root element is missing.")
                   File.WriteAllText("Connections.xml", string.Empty);
            }
        }
        private void AddAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            var form = new ConnectionAddition();
            form.ShowDialog();
            form = null;
        }
        private async void DelAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {//todo: оптимизировать запросы, выборки
            var selectedItem = ProviderListView.SelectedItem as Connection;
            if (PortfolioListView.Items.Cast<Common.Entities.Portfolio>().Any(i => i.Connection.Id == selectedItem.Id))
            {

                //var dialog = new BaseMetroDialog(MainFrm.MainFrm); //(BaseMetroDialog)this.Resources["CustomDialogTest"];
                //await this.ShowMetroDialogAsync(dialog);

                //textBlock.Text = "The dialog will close in 2 seconds.";
                await Task.Delay(2000);

//                await this.HideMetroDialogAsync(dialog);
                //MessageBox.Show(this, @"На данном соединении завязан портфель, удаление невозможно!");
                //return;
            }
            MessageBoxResult result = MessageBox.Show("Connection \"{0}\" will be deleted! You sure?".Put(ProviderListView.SelectedItem),"Delete connection", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in ProviderListView.SelectedItems.Cast<Connection>().ToList())
                {
                    //if (PortfolioListView.Items.Cast<Common.Entities.Portfolio>().Any(i => i.Connection.DisplayName == item.DisplayName))
                    //{
                    //    MessageBox.Show(this, @"На данном соединении завязан портфель, удаление невозможно!");
                    //    return;
                    //}
                    ConnectionsStorage.Remove(item);
                    SaveProviderItems();
                    var connection = ConnectionManager.Connections.FirstOrDefault(i => i.ConnectionName == item.DisplayName);
                    if (connection != null)
                    {
                        connection.Disconnect();
                        connection.Dispose();
                        ConnectionManager.Connections.Remove(connection);
                        Logger.Info("Connection \"{0}\" has been deleted", connection.ConnectionName);
                    }
                }
            }
        }
        private void EditAgentConnectionBtnClick(object sender, RoutedEventArgs e)
        {
            var listToEdit = ProviderListView.SelectedItems.Cast<Connection>().ToList();
            if (listToEdit[0].ConnectionParams.ConnectionState == ConnectionParams.ConnectionStatus.Connected)
            {

                //todo: заменить на неактивную кнопку редактирования, убрать предупреждения
                MessageBox.Show(this, @"Активное соединение, редактирование невозможно!");
                return;
            }
            //TODO: переделать
            foreach (var connectionEditWindow in from agentSettings in listToEdit
                                                 let index = ConnectionsStorage.IndexOf(agentSettings)
                                                 where index != -1
                                                 select new ConnectionAddition(agentSettings, index))
            {
                connectionEditWindow.Title = "Аист Трейдер - Редактировать подключение";
                connectionEditWindow.ShowDialog();
                connectionEditWindow.Close();
            }
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
            //    var rowItem = ConnectionsStorage.FirstOrDefault(i => i == item);
            //    rowItem.Connection.IsActive = false;
            //}
        }

        public void UpdateProviderListView()
        {
            ProviderListView.ItemsSource = ConnectionsStorage;
            ProviderCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(ProviderListView.ItemsSource);
            ProviderCollectionView.Refresh();
            //ICollectionView view = CollectionViewSource.GetDefaultView(Instance.ProviderListView.ItemsSource);
            //view.Refresh();
        }
        public void UpdateProviderGridListView(Connection agentConnection)
        {
            var item = ConnectionsStorage.FirstOrDefault(i => i.DisplayName == agentConnection.DisplayName);
            item.ConnectionParams.VariationMargin = item.ConnectionParams.Accounts.FirstOrDefault().VariationMargin;
            item.ConnectionParams.Funds = item.ConnectionParams.Accounts.FirstOrDefault().CurrentValue;
            item.ConnectionParams.NetValue= item.ConnectionParams.Accounts.FirstOrDefault().CurrentPrice;


            ProviderListView.ItemsSource = ConnectionsStorage;
            ProviderCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(ProviderListView.ItemsSource);
            ProviderCollectionView.Refresh();
        }

        public void ConnectAccount(Connection agent)
        {
            string ipEndPoint = "";
            bool sLoaded=false;

            if (agent.ConnectionParams.PlazaConnectionParams.IpEndPoint == null)
            {
                try
                {
                    Logger.Info("Trying to get the ip,port of plaza connection -\"{0}\"...", agent.DisplayName.ToString());
                    ipEndPoint = GetPlazaConnectionIpPort(agent.ConnectionParams.PlazaConnectionParams.Path);
                    Logger.Info("IP and port of -\"{0}\" connection were successfully acquired", agent.DisplayName.ToString());
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e.Message);
                    Logger.Log(LogLevel.Error, e.InnerException.Message);
                }
                var item = ConnectionsStorage.Cast<Connection>().Where(i => i.ConnectionParams.PlazaConnectionParams.Path == agent.ConnectionParams.PlazaConnectionParams.Path)
                        .Select(i => i).FirstOrDefault();
                item.ConnectionParams.PlazaConnectionParams.IpEndPoint = ipEndPoint;
            }
            else
                ipEndPoint = agent.ConnectionParams.PlazaConnectionParams.IpEndPoint;
            var connection = new AistTraderConnnectionWrapper(agent.DisplayName) {Address = ipEndPoint.To<IPEndPoint>(), IsCGate = true, IsDemo = true};

            //TODO: посмотри примеры того как идет динамический апдейт, а потом уже подписывай события на то что будет апдейтится
            agent.ConnectionParams.Accounts = new List<StockSharp.BusinessEntities.Portfolio>();
            agent.ConnectionParams.Tools = new List<Security>();
            agent.ConnectionParams.IsRegistredConnection = true;
            connection.NewPortfolios += portfolios =>
            {
                this.GuiAsync(() => agent.ConnectionParams.Accounts.AddRange(portfolios))/* PortfoliosList.AddRange(portfolios))*/;
                this.GuiAsync(() => UpdateProviderGridListView(agent));
                this.GuiAsync(() => Logger.Info("Portfolios for connection \"{0}\" were loaded",connection.ConnectionName));
                //try
                //{
                //    TimeHelper.SyncMarketTime();
                //}
                //catch (Exception)
                //{
                //}
            };
            connection.NewSecurities += securities =>
            {
                this.GuiAsync(() =>
                {
                    agent.ConnectionParams.Tools.AddRange(securities)/* PortfoliosList.AddRange(portfolios))*/;
                    if (agent.ConnectionParams.Tools.Count > 200 && !sLoaded)
                    {
                        sLoaded = true;
                        Logger.Info("Securities were loaded");
                    }
                });
            };
            connection.Connected += () =>
            {
                this.GuiAsync(() => agent.ConnectionParams.IsConnected = true);
                this.GuiAsync(() => agent.ConnectionParams.ConnectionState = ConnectionParams.ConnectionStatus.Connected);
                this.GuiAsync(() => UpdateProviderListView());
                this.GuiAsync(() => Logger.Info("Connection - \"{0}\" is active now", connection.ConnectionName));
            };
            connection.Disconnected += () =>
            {
                this.GuiAsync(() => agent.ConnectionParams.IsConnected = false);
                this.GuiAsync(() => agent.ConnectionParams.ConnectionState = ConnectionParams.ConnectionStatus.Disconnected);   
                this.GuiAsync(() => UpdateProviderListView());
                this.GuiAsync(() => Logger.Info("Connection - \"{0}\" is not active now", connection.Name));
            };
            connection.ConnectionError += error =>
            {
                this.GuiAsync(() => Logger.Info("Connection error -\"{0}\", connection \"{1}\" is not active", error, connection.Name));
                //todo: коды ошибок в енам, подкидывать готовые уведомления на примере нерабочего роутера, подсвечивать в гриде цветом, если ошибка критическая
            };
            //TODO: Добавить все эвенты по аналогии с портфелями
            connection.Connect();
            ConnectionManager.Add(connection);
            #region Trash
            //только в боевоей версии
            //Trader.CGateKey = "C99ElZcac2yZzSC9xSYqyaq8xXAnNrW";
            #endregion
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
                //ProviderListView.SelectedItems.Clear();
                //ProviderListView.SelectedItems.Add(item);
                var rowItem = Instance.ConnectionsStorage.FirstOrDefault(i => i == item);
                int index = ConnectionManager.Connections.FindIndex(i => i.ConnectionName == rowItem.DisplayName);
                rowItem.ConnectionParams.Command = OperationCommand.Disconnect;
                if (rowItem.ConnectionParams.IsRegistredConnection)
                    ConnectionManager.Connections[index].Connect();
                else
                    ConnectAccount(item as Connection);
                UpdateProviderListView();
            }
            else
            {
                var item = (sender as FrameworkElement).DataContext;
                var rowItem = ConnectionsStorage.FirstOrDefault(i => i == item);
                var con = ConnectionManager.Connections.FirstOrDefault(m => rowItem != null && m.ConnectionName == rowItem.ToString());
                //get index from manager by name
                int index = ConnectionManager.Connections.FindIndex(i => i.ConnectionName == rowItem.DisplayName);
                ConnectionManager.Connections[index].Disconnect();
                
                if (con != null)
                {
                    //con.Disconnect();
                    //con.Dispose();
                }
                if (rowItem != null)
                {
                    rowItem.ConnectionParams.Command = OperationCommand.Connect;
                    //rowItem.ConnectionParams.IsRegistredConnection = false;
                }
                UpdateProviderListView();
            }
        }
        private void ProviderListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsProviderSettingsLoaded & (File.Exists("Connections.xml")) & ConnectionsStorage.Count == 0)
                InitiateProviderItems();


            if (ProviderListView.Items.Count ==0)
            {
                EditAgentConnectionBtn.IsEnabled = false;
                DelAgentConnectionBtn.IsEnabled = false;
            }
            else
            {
                EditAgentConnectionBtn.IsEnabled = true;
                DelAgentConnectionBtn.IsEnabled = true;
            }
        }

        private void ProviderListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProviderListView.Items.Count == 0)
            {
                EditAgentConnectionBtn.IsEnabled = false;
                DelAgentConnectionBtn.IsEnabled = false;
            }
            else
            {
                EditAgentConnectionBtn.IsEnabled = true;
                DelAgentConnectionBtn.IsEnabled = true;
            }
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
