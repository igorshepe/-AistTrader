using System.ComponentModel;
using Strategies.Common;

namespace Strategies.Settings
{
    public abstract class StrategyDefaultSettings : INotifyPropertyChanged
    {
        public abstract void Load(SerializableDictionary<string, object> settingsStorage);
        public abstract SerializableDictionary<string, object> Save();
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
