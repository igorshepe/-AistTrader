using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Common.Entities;
using Ecng.Common;
using Ecng.Xaml;
using MoreLinq;
using StockSharp.Algo;
using Strategies.Common;
using Strategies.Settings;

namespace AistTrader
{



    public class AgentSettingParametrProperty
    {
        public  string Name { get; set; }
        public  decimal Parametr { get; set; }
        public  bool UseInAgentName { get; set; }
    }
    public partial class AgentSettings
    {
        public SerializableDictionary<string, object> SettingsStorage { get; private set; }
        // ReSharper disable MemberCanBePrivate.Global
        public StrategyDefaultSettings Settings { get; set; }
        public CollectionView AgentSettingsCollectionView { get; set; }
        public ObservableCollection<object> AgentSettingsStorage { get; private set; }

        // ReSharper restore MemberCanBePrivate.Global

        public AgentSettings(SerializableDictionary<string, object> settingsStorage, StrategyDefaultSettings settings)
        {
            AgentSettingsStorage = new ObservableCollection<object>();

            SettingsStorage = settingsStorage;
            Settings = settings;
            InitializeComponent();
            Settings.Load(SettingsStorage);
            Type type = typeof (AistInvestStrategySettings);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var agentSettingsProperty = new AgentSettingParametrProperty();
                agentSettingsProperty.Name = property.Name;
                if (property.Name == "TimeFrame")
                {
                    TimeSpan test =(TimeSpan)property.GetValue(Settings);
                    double sec = test.TotalSeconds;
                    agentSettingsProperty.Parametr = (decimal)sec;
                    //agentSettingsProperty.Parametr = test;
                    //                    agentSettingsProperty.Parametr = (decimal)property.GetValue(Settings);
                }
                else
                    agentSettingsProperty.Parametr = (decimal)property.GetValue(Settings);
                
                agentSettingsProperty.UseInAgentName = false;
                AgentSettingsStorage.Add(agentSettingsProperty);
            }
            AgentSettingsDG.ItemsSource = AgentSettingsStorage;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsStorage = Settings.Save();
            if (SettingsStorage == null) return;
            DialogResult = true;
        }
    }
}
