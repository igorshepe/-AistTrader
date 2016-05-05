﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strategies.Common;
using Strategies.Strategies;

namespace Strategies.Settings
{
    public static class CandleStrategyDefaultSettings
    {
        public const string TimeFrameString = "TimeFrame";
        public static readonly TimeSpan TimeFrame = new TimeSpan(0, 5, 0);
    }
    class CandleStrategySettings: StrategyDefaultSettings
    {
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

        public override void Load(SerializableDictionary<string, object> settingsStorage)
        {
            TimeFrame = settingsStorage != null && settingsStorage.ContainsKey(CandleStrategyDefaultSettings.TimeFrameString)
                        ? TimeSpan.Parse((string)settingsStorage[CandleStrategyDefaultSettings.TimeFrameString])
                        : CandleStrategyDefaultSettings.TimeFrame;
          
        }

        public override SerializableDictionary<string, object> Save()
        {
           

            var settings = new SerializableDictionary<string, object>
                               {
                                   {CandleStrategyDefaultSettings.TimeFrameString, TimeFrame.ToString()}
                                   
                               };

            return settings;
        }
    }
}
