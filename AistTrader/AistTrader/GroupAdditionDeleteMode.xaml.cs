﻿using System;
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
        public string AgentName { get; set; }
        public bool IsCancelled { get; set; }
        private int EditIndex { get; set; }

        public GroupAdditionDeleteMode(string agentName)
        {
            InitializeComponent();
            DataContext = this;
            EditIndex = int.MinValue;
            AgentName = agentName;
            LoadParams();
        }

        private void LoadParams()
        {
            textBlock.Text = "Select presented options for: " + AgentName;
        }

        private void OkBtnClickEvent(object sender, RoutedEventArgs e)
        {
            if (ClosePositionsAndDeleteRb.IsChecked == true)
            {
                SelectedDeleteMode = AgentManagerDeleteMode.ClosePositionsAndDelete;
            }
            if (WaitAndCloseRb.IsChecked == true)
            {
                SelectedDeleteMode = AgentManagerDeleteMode.WaitForClosingAndDeleteAfter;
            }
            Close();
            IsCancelled = false;
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
            IsCancelled = true;
        }
    }
}
