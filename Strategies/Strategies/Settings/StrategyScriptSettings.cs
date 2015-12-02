using System;
using System.ComponentModel;
using Strategies.Common;

namespace Strategies.Settings
{
    public static class StrategyScriptDefaultSettings
    {
        public const string TimeFrameString = "TimeFrame";
        public static readonly TimeSpan TimeFrame = new TimeSpan(0, 15, 0);

        public const string StopLongString = "StopLong";
        public const decimal StopLong = 1111;

        public const string StopShortString = "StopShort";
        public const decimal StopShort = 1666;
    }


    public class StrategyScriptSettings : StrategyDefaultSettings
    {
        // ReSharper disable MemberCanBePrivate.Global
        private TimeSpan _timeFrame;

        [DisplayName(@"Таймфрейм")]
        public TimeSpan TimeFrame
        {
            get { return _timeFrame; }
            set
            {
                if (_timeFrame != value)
                {
                    _timeFrame = value;
                    NotifyPropertyChanged("TimeFrame");
                }
            }
        }

        private decimal _stopLong;

        [DisplayName(@"Стоп лонг")]
        public decimal StopLong
        {
            get { return _stopLong; }
            set
            {
                if (_stopLong != value)
                {
                    _stopLong = value;
                    NotifyPropertyChanged("StopLong");
                }
            }
        }

        private decimal _stopShort;

        [DisplayName(@"Стоп шорт")]
        public decimal StopShort
        {
            get { return _stopShort; }
            set
            {
                if (_stopShort != value)
                {
                    _stopShort = value;
                    NotifyPropertyChanged("StopShort");
                }
            }
        }


        // ReSharper restore MemberCanBePrivate.Global

        public override void Load(SerializableDictionary<string, object> settingsStorage)
        {
            TimeFrame = settingsStorage != null && settingsStorage.ContainsKey(StrategyScriptDefaultSettings.TimeFrameString)
                        ? TimeSpan.Parse((string)settingsStorage[StrategyScriptDefaultSettings.TimeFrameString])
                        : StrategyScriptDefaultSettings.TimeFrame;
            StopLong = settingsStorage != null && settingsStorage.ContainsKey(StrategyScriptDefaultSettings.StopLongString)
                        ? (decimal)settingsStorage[StrategyScriptDefaultSettings.StopLongString]
                        : StrategyScriptDefaultSettings.StopLong;
            StopShort = settingsStorage != null && settingsStorage.ContainsKey(StrategyScriptDefaultSettings.StopShortString)
                            ? (decimal)settingsStorage[StrategyScriptDefaultSettings.StopShortString]
                            : StrategyScriptDefaultSettings.StopShort;
        }

        public override SerializableDictionary<string, object> Save()
        {
            if (StopLong <= 0 || StopShort <= 0)
            {
                //MessageBox.Show(@"Задайте значение больше 0.");
                return null;
            }

            var settings = new SerializableDictionary<string, object>
			               	{
								{StrategyScriptDefaultSettings.TimeFrameString, TimeFrame.ToString()},
			               		{StrategyScriptDefaultSettings.StopLongString, StopLong},
			               		{StrategyScriptDefaultSettings.StopShortString, StopShort}
			               	};

            return settings;
        }
    }
}
