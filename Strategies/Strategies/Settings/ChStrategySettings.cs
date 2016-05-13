using System;
using System.ComponentModel;
using Strategies.Common;

namespace Strategies.Settings
{
    public static class ChStrategyDefaultSettings
    {
        public const string TimeFrameString = "TimeFrame";
        public static readonly TimeSpan TimeFrame = new TimeSpan(0, 15, 0);

        public const string FastSmaString = "FastSma";
        public const decimal FastSma = 50;

        public const string SlowSmaString = "SlowSma";
        public const decimal SlowSma = 100;

        public const string PeriodString = "Period";
        public const decimal Period = 100;
    }


    public class ChStrategySettings : StrategyDefaultSettings
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

        private decimal _fastSma;

        [DisplayName(@"Fast SMA")]
        public decimal FastSma
        {
            get { return _fastSma; }
            set
            {
                if (_fastSma != value)
                {
                    _fastSma = value;
                    NotifyPropertyChanged("FastSma");
                }
            }
        }

        private decimal _slowSma;

        [DisplayName(@"Slow SMA")]
        public decimal SlowSma
        {
            get { return _slowSma; }
            set
            {
                if (_slowSma != value)
                {
                    _slowSma = value;
                    NotifyPropertyChanged("SlowSma");
                }
            }
        }

        private decimal _period;

        [DisplayName(@"Период")]
        public decimal Period
            {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    NotifyPropertyChanged("Period");
                }
            }
        }

        // ReSharper restore MemberCanBePrivate.Global

        public override void Load(SerializableDictionary<string, object> settingsStorage)
        {
            TimeFrame = settingsStorage != null && settingsStorage.ContainsKey(ChStrategyDefaultSettings.TimeFrameString)
                        ? TimeSpan.Parse((string)settingsStorage[ChStrategyDefaultSettings.TimeFrameString])
                        : ChStrategyDefaultSettings.TimeFrame;
            FastSma = settingsStorage != null && settingsStorage.ContainsKey(ChStrategyDefaultSettings.FastSmaString)
                        ? (decimal)settingsStorage[ChStrategyDefaultSettings.FastSmaString]
                        : ChStrategyDefaultSettings.FastSma;
            SlowSma = settingsStorage != null && settingsStorage.ContainsKey(ChStrategyDefaultSettings.SlowSmaString)
                            ? (decimal)settingsStorage[ChStrategyDefaultSettings.SlowSmaString]
                            : ChStrategyDefaultSettings.SlowSma;
            Period = settingsStorage != null && settingsStorage.ContainsKey(ChStrategyDefaultSettings.PeriodString)
                            ? (decimal)settingsStorage[ChStrategyDefaultSettings.PeriodString]
                            : ChStrategyDefaultSettings.Period;
        }

        public override SerializableDictionary<string, object> Save()
        {
            if (FastSma <= 0 || SlowSma <= 0)
            {
                //MessageBox.Show(@"Задайте значение больше 0.");
                return null;
            }

            var settings = new SerializableDictionary<string, object>
                               {
                                {ChStrategyDefaultSettings.TimeFrameString, TimeFrame.ToString()},
                                   {ChStrategyDefaultSettings.FastSmaString, FastSma},
                                   {ChStrategyDefaultSettings.SlowSmaString, SlowSma},
                                {ChStrategyDefaultSettings.PeriodString, Period}
                               };

            return settings;
        }
    }
}
