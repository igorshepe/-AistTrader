using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using AistTrader.Properties;
using Common.Entities;
using Common.Params;
using Ecng.Common;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace AistTrader
{
    [TypeConverter(typeof(PropertySort))]//
    public class TradeSettingPGrid
    {
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Исполнять входы сразу"),
            PropertyOrder(1)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool DirectIns
        {
            get;
            set;
        }

        [CategoryAttribute("Исполнение агента"),
        DisplayName("Исполнять выходы сразу"),
            PropertyOrder(2)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool DirectOuts
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Действия автоотрытия(баров)"),
            PropertyOrder(3)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public string AutoOpenBarAction
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Действия автозакрытия(баров)"),
            PropertyOrder(4)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public string AutoCloseBarAction
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Игнорировать позиции вне истории"),
            PropertyOrder(5)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool IgnorePositionOutOfHistory
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Увед. о проп. входах"),
            PropertyOrder(6)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool NotifyOnMissedIns
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Не открывать, есть есть пропуск выхо.."),
            PropertyOrder(7)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool NotOpenIfGap
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Не уведом. пересчет"),
            PropertyOrder(8)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool NotifyOnRecount
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Виртуальная позиция макс. свечей"),
            PropertyOrder(9)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public int VirtualCandleMax
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Ждать исполнения выхода"),
            PropertyOrder(10)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public int WaitOnSuccessfulOut
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Ждать исполнения входа"),
            PropertyOrder(11)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public int WaitOnSuccessfulIn
        {
            get;
            set;
        }
        [CategoryAttribute("Исполнение агента"),
        DisplayName("Имит. очередность позиций"),
            PropertyOrder(12)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool SimulatePositionSequence
        {
            get;
            set;
        }

        [CategoryAttribute("Выставление заявок"),
        DisplayName("Проскальз. в шагах"),
            PropertyOrder(1)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public int SlippingInSteps
        {
            get;
            set;
        }
        [CategoryAttribute("Выставление заявок"),
        DisplayName("Проскальз. в %"),
            PropertyOrder(2)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public int SlippingInPercent
        {
            get;
            set;
        }
        [CategoryAttribute("Выставление заявок"),
        DisplayName("Take profit без проскальзывания"),
            PropertyOrder(3)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool TakeProfitWithNoSlipping
        {
            get;
            set;
        }
        [CategoryAttribute("Выставление заявок"),
        DisplayName("Открытие лимитными заявками"),
            PropertyOrder(4)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool OpeningWithLimitingOrders
        {
            get;
            set;
        }
        [CategoryAttribute("Выставление заявок"),
        DisplayName("\"По рынку\" с фикс. ценой"),
            PropertyOrder(5)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool ByMarketWithFixedPrice
        {
            get;
            set;
        }
        [CategoryAttribute("Выставление заявок"),
        DisplayName("\"Плохие\" заявки по рынку"),
            PropertyOrder(6)]
        //DescriptionAttribute("Test Description")] todo: запросить описание
        public bool BadOrdersByMarket
        {
            get;
            set;
        }
    }
    public partial class ManagerAdditionTradeSettings
    {
        private int EditIndex { get; set; }
        public ManagerAdditionTradeSettings()
        {
            DataContext = this;
            InitializeComponent();
            EditIndex = int.MinValue;
            TradeSettingPGrid traderSettingPGrid = new TradeSettingPGrid();
            PropertyGridControl.SelectedObject = traderSettingPGrid;
        }
        public ManagerAdditionTradeSettings(Connection account, int editIndex)
        {
            InitializeComponent();
            InitFields(account);
            DataContext = this;
            EditIndex = editIndex;
        }
        private void InitFields(Connection account)
        {
        }
        private void OkBtnClick(object sender, RoutedEventArgs e)
        { 
        }
    }
}
