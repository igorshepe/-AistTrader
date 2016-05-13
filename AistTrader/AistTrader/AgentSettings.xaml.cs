using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    [Serializable]
    public class AgentSettingParameterProperty
    {
        public  string Name { get; set; }
        //todo: object tests

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
            var type  = settings.GetType();
            
            InitializeComponent();
            Settings.Load(SettingsStorage);
            //Type type = typeof (CandleStrategySettings);
            PropertyInfo[] properties = type.GetProperties();   
            foreach (PropertyInfo property in properties)
            {
                var agentSettingsProperty = new AgentSettingParameterProperty();
                agentSettingsProperty.Name = property.Name;
                
                if (property.Name == "TimeFrame")
                {
                    TimeSpan ts = (TimeSpan)property.GetValue(Settings);
                    agentSettingsProperty.Parametr = (decimal)ts.TotalSeconds;

                   // if (SettingsStorage != null) SettingsStorage.Add(property.Name, ts);
                }
                
                //{
                    //agentSettingsProperty.Parametr = test;
                    //                    agentSettingsProperty.Parametr = (decimal)property.GetValue(Settings);
                //}
                else
                    agentSettingsProperty.Parametr = (decimal)property.GetValue(Settings);

                agentSettingsProperty.UseInAgentName = false;
                AgentSettingsStorage.Add(agentSettingsProperty);
            }
            AgentSettingsDG.ItemsSource = AgentSettingsStorage;

            

        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsStorage.Clear();
            foreach (AgentSettingParameterProperty sett in AgentSettingsStorage)
            {
                SettingsStorage.Add(sett.Name, sett.Parametr);
            }
            //SettingsStorage = Settings.Save();
            if (SettingsStorage == null) return;
            DialogResult = true;
        }
    }
}
