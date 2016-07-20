using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using StockSharp.Algo;
using StockSharp.Algo.Storages;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    public partial class GroupAdditionSecurityPicker
    {
        private Agent AgentToEdit;
        public string SelectedSecurity;
        private int EditIndex { get; set; }
        public GroupAdditionSecurityPicker(Agent agent)
        {
            InitializeComponent();
            AgentToEdit = agent;
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }

        private void LoadParams()
        {
            AgentNameLbl.Content = AgentToEdit.Name;
            //todo: вынести в алгоритм добавления
            var AnyActiveConnection =MainWindow.Instance.ConnectionManager.Any(i => i.ConnectionState == ConnectionStates.Connected);
            if (AnyActiveConnection)
            {
                SecurityPickerSS.SecurityProvider = new CollectionSecurityProvider(MainWindow.Instance.ConnectionManager.FirstOrDefault(i => i.ConnectionState == ConnectionStates.Connected).Securities);
            }
            else
            {
                var firstOrDefault = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.ConnectionParams.Tools != null);
                if (firstOrDefault != null)
                {
                    SecurityPickerSS.SecurityProvider = new CollectionSecurityProvider(firstOrDefault.ConnectionParams.Tools.ToList());
                }
                if (firstOrDefault == null)
                {
                    MessageBox.Show("No cashed or live securities.");
                }
            }
        }

        private void AttachSecForAgentBtnClick(object sender, RoutedEventArgs e)
        {

            //AgentToEdit.Params.Security= SecurityPickerSS.SelectedSecurity.Code;
            SelectedSecurity = SecurityPickerSS.SelectedSecurity.Code;
            //SecurityPickerSS.SecurityProvider.Dispose();
            //SecurityPickerSS.SecurityProvider = null;
            
            Close();
        }
        private void SecurityPickerSS_OnSecuritySelected()
        {
            if (SecurityPickerSS.SelectedSecurity != null )
            {
                OkBtnClick.IsEnabled = true;
            }
        }

        private void GroupAdditionSecurityPicker_OnClosing(object sender, CancelEventArgs e)
        {
            if (SecurityPickerSS.SelectedSecurity == null)
            {
                MessageBox.Show("Security is not selected.");
                e.Cancel = true;
            }
        }
    }
}
