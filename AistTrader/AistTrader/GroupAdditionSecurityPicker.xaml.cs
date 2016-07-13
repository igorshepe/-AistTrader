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
            SecurityPickerSS.SecurityProvider =new CollectionSecurityProvider(MainWindow.Instance.ConnectionManager.FirstOrDefault(i => i.ConnectionState == ConnectionStates.Connected).Securities);
        }

        private void AttachSecForAgentBtnClick(object sender, RoutedEventArgs e)
        {

            //AgentToEdit.Params.Security= SecurityPickerSS.SelectedSecurity.Code;
            SelectedSecurity = SecurityPickerSS.SelectedSecurity.Code;
            
           
            SecurityPickerSS.SecurityProvider.Dispose();
            SecurityPickerSS.SecurityProvider = null;
            
            Close();
        }
        private void SecurityPickerSS_OnSecuritySelected()
        {
            if (SecurityPickerSS.SelectedSecurity != null )
            {
                OkBtnClick.IsEnabled = true;
            }
        }
    }
}
