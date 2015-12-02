using System.Windows;
using Strategies.Common;
using Strategies.Settings;

namespace AistTrader
{
    public partial class StrategiesSettingsWindow
    {
        public SerializableDictionary<string, object> SettingsStorage { get; private set; }
        // ReSharper disable MemberCanBePrivate.Global
        public StrategyDefaultSettings Settings { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        public StrategiesSettingsWindow(SerializableDictionary<string, object> settingsStorage, StrategyDefaultSettings settings)
        {
            SettingsStorage = settingsStorage;
            Settings = settings;
            InitializeComponent();
            Settings.Load(SettingsStorage);
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsStorage = Settings.Save();
            if (SettingsStorage == null) return;
            DialogResult = true;
        }
    }
}
