using System;
using System.ComponentModel;
using Strategies.Common;

namespace Strategies.Settings
{
    public static class AistInvestStrategyDefaultSettings
    {
        public const string TimeFrameString = "TimeFrame";
        public static readonly TimeSpan TimeFrame = new TimeSpan(0, 15, 0);

        public const string StopLongString = "StopLong";
        public const decimal StopLong = 1111;

        public const string StopShortString = "StopShort";
        public const decimal StopShort = 1666;

        public const string ConstString = "Const";
        public const decimal Const = 123;
    }


    public class AistInvestStrategySettings : StrategyDefaultSettings
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

        private decimal _const;

        [DisplayName(@"Константа")]
        public decimal Const
        {
            get { return _const; }
            set
            {
                if (_const != value)
                {
                    _const = value;
                    NotifyPropertyChanged("Const");
                }
            }
        }

        // ReSharper restore MemberCanBePrivate.Global

        public override void Load(SerializableDictionary<string, object> settingsStorage)
        {
            TimeFrame = settingsStorage != null && settingsStorage.ContainsKey(AistInvestStrategyDefaultSettings.TimeFrameString)
                        ? TimeSpan.Parse((string)settingsStorage[AistInvestStrategyDefaultSettings.TimeFrameString])
                        : AistInvestStrategyDefaultSettings.TimeFrame;
            StopLong = settingsStorage != null && settingsStorage.ContainsKey(AistInvestStrategyDefaultSettings.StopLongString)
                        ? (decimal)settingsStorage[AistInvestStrategyDefaultSettings.StopLongString]
                        : AistInvestStrategyDefaultSettings.StopLong;
            StopShort = settingsStorage != null && settingsStorage.ContainsKey(AistInvestStrategyDefaultSettings.StopShortString)
                            ? (decimal)settingsStorage[AistInvestStrategyDefaultSettings.StopShortString]
                            : AistInvestStrategyDefaultSettings.StopShort;
            Const = settingsStorage != null && settingsStorage.ContainsKey(AistInvestStrategyDefaultSettings.ConstString)
                            ? (decimal)settingsStorage[AistInvestStrategyDefaultSettings.ConstString]
                            : AistInvestStrategyDefaultSettings.Const;
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
								{AistInvestStrategyDefaultSettings.TimeFrameString, TimeFrame.ToString()},
			               		{AistInvestStrategyDefaultSettings.StopLongString, StopLong},
			               		{AistInvestStrategyDefaultSettings.StopShortString, StopShort},
                                {AistInvestStrategyDefaultSettings.ConstString, Const}
			               	};

            return settings;
        }
    }
}
