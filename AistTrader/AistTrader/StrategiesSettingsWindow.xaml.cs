﻿using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Common.Entities;
using Ecng.Xaml;
using Strategies.Common;
using Strategies.Settings;

namespace AistTrader
{



    public class AgentSettingParametrProperty
    {
        public  string Name { get; set; }
        public  object Parametr { get; set; }
        public  bool UseInAgentName { get; set; }
    }
    public partial class StrategiesSettingsWindow
    {
        public SerializableDictionary<string, object> SettingsStorage { get; private set; }
        // ReSharper disable MemberCanBePrivate.Global
        public StrategyDefaultSettings Settings { get; set; }
        public CollectionView AgentSettingsCollectionView { get; set; }
        public ObservableCollection<object> AgentSettingsStorage { get; private set; }

        // ReSharper restore MemberCanBePrivate.Global

        public StrategiesSettingsWindow(SerializableDictionary<string, object> settingsStorage, StrategyDefaultSettings settings)
        {
            AgentSettingsStorage = new ObservableCollection<object>();
            var strat = new AistInvestStrategySettings();

            var agentSettingsProperty = new AgentSettingParametrProperty();
            


            //AgentSettingsStorage.Add();


            //AgentSettingsDG.ItemsSource = AgentSettingsStorage;

            //AgentSettingsStorage.Add();


            SettingsStorage = settingsStorage;
            Settings = settings;
            InitializeComponent();
            Settings.Load(SettingsStorage);
            Type type = typeof (AistInvestStrategySettings);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                agentSettingsProperty.Name = property.Name;
                agentSettingsProperty.Parametr = property.GetValue(Settings);
                agentSettingsProperty.UseInAgentName = false;
                AgentSettingsStorage.Add(agentSettingsProperty);
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsStorage = Settings.Save();
            if (SettingsStorage == null) return;
            DialogResult = true;
        }
    }
}
