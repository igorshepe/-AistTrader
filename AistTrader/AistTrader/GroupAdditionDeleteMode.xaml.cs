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
using static Common.Params.ManagerParams;

namespace AistTrader
{
    public partial class GroupAdditionDeleteMode
    {
        public AgentManagerDeleteMode SelectedDeleteMode;
        private int EditIndex { get; set; }
        public GroupAdditionDeleteMode()
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            LoadParams();
        }

        private void LoadParams()
        {

        }
        private void OkBtnClickEvent(object sender, RoutedEventArgs e)
        {
            if (ClosePositionsAndDeleteRb.IsChecked == true)
                SelectedDeleteMode = AgentManagerDeleteMode.ClosePositionsAndDelete;
            if (WaitAndCloseRb.IsChecked== true)
                SelectedDeleteMode = AgentManagerDeleteMode.WaitForClosingAndDeleteAfter;
            Close();
        }

        private void ClosePositionsAndDeleteRb_Checked(object sender, RoutedEventArgs e)
        {
            OkBtnClick.IsEnabled = true;
        }
        private void WaitAndCloseRb_Checked(object sender, RoutedEventArgs e)
        {
            OkBtnClick.IsEnabled = true;
        }
        private void StratSettings_Closing(object sender, CancelEventArgs e)
        {
            if (ClosePositionsAndDeleteRb.IsChecked ==false & WaitAndCloseRb.IsChecked == false)
            {
                MessageBox.Show("Delete mode is not selected!");
                e.Cancel = true;
            }
            
        }
    }
}
