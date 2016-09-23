using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Strategies.Common;
using Strategies.Settings;

namespace AistTrader
{
    [Serializable]
    public class AgentSettingParameterProperty
    {
        public AgentSettingParameterProperty()
        {
            UseInAgentName = true;
        }

        public override string ToString()
        {
            return Name;
        }

        public  string Name { get; set; }
        //todo: object tests
        public  decimal Parametr { get; set; }
        public  bool UseInAgentName { get; set; }
    }

    public partial class AgentSettings
    {
        public SerializableDictionary<string, object> SettingsStorage { get; private set; }
        public StrategyDefaultSettings Settings { get; set; }
        public CollectionView AgentSettingsCollectionView { get; set; }
        public static ObservableCollection<object> AgentSettingsStorage { get; private set; }

        public AgentSettings(SerializableDictionary<string, object> settingsStorage, StrategyDefaultSettings settings)
        {
            AgentSettingsStorage = new ObservableCollection<object>();

            Settings = settings;
            var type  = settings.GetType();
            
            InitializeComponent();
            Settings.Load(settingsStorage ?? SettingsStorage);
            PropertyInfo[] properties = type.GetProperties();   
            foreach (PropertyInfo property in properties)
            {
                var agentSettingsProperty = new AgentSettingParameterProperty();
                agentSettingsProperty.Name = property.Name;

                if (property.Name == "TimeFrame")
                {
                    TimeSpan ts = (TimeSpan)property.GetValue(Settings);
                    agentSettingsProperty.Parametr = (decimal)ts.TotalSeconds;
                }
                else
                {
                    agentSettingsProperty.Parametr = (decimal)property.GetValue(Settings);
                }

                AgentSettingsStorage.Add(agentSettingsProperty);
            }
            AgentSettingsDG.ItemsSource = AgentSettingsStorage;
            FillAgentSettings();
        }

        public void FillAgentSettings()
        {
            SettingsStorage = new SerializableDictionary<string, object>();
            foreach (AgentSettingParameterProperty sett in AgentSettingsStorage)
            {
                if (sett.Name == "TimeFrame")
                {
                    TimeSpan TimeFrame = new TimeSpan(0, 0, int.Parse(sett.Parametr.ToString()));
                    SettingsStorage.Add(sett.Name, TimeFrame.TotalSeconds);
                }
                else
                {
                    SettingsStorage.Add(sett.Name, sett.Parametr);
                }
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            FillAgentSettings();
            if (SettingsStorage == null) { return; }
            DialogResult = true;
        }
    }
}
