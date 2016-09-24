
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using Common.Entities;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace AistTrader
{
    [TypeConverter(typeof(PropertySort))]
    public class TradeSettingPGrid
    {
        [CategoryAttribute("Исполнение агента"), DisplayName("Исполнять входы сразу"), PropertyOrder(1)]
        public bool EnterStart
        {
            get { return true; }
            set { EnterStart = value; } 
        }

        [CategoryAttribute("Исполнение агента"), DisplayName("Исполнять выходы сразу"), PropertyOrder(2)]
        public bool ExiteStart
        {
            get { return true; }
            set { ExiteStart = value; }
        }
        [CategoryAttribute("Исполнение агента"), DisplayName("Действия автооткрытия(баров)"), PropertyOrder(3)]
        public int AutoOpenBar { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Действия автозакрытия(баров)"), PropertyOrder(4)]
        public int AutoCloseBar { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Игнорировать позиции вне истории"), PropertyOrder(5)]
        public bool IgnoreHistory
        {
            get { return true; }
            set { IgnoreHistory = value; }
        }
        [CategoryAttribute("Исполнение агента"), DisplayName("Увед. о проп. входах"), PropertyOrder(6)]
        public bool PostMissingDeal
        {
            get { return true; }
            set { PostMissingDeal = value; }
        }
        [CategoryAttribute("Исполнение агента"), DisplayName("Не открывать, если есть пропуск выхо.."), PropertyOrder(7)]
        public bool NoEnterWaitExit { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Не уведом. пересчет"), PropertyOrder(8)]
        public bool NoPostRecalculation { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Виртуальная позиция макс. свечей"), PropertyOrder(9)]
        public int VirtPosMaxCandles { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Ждать исполнения выхода"), PropertyOrder(10)]
        public int WaitLimitExit { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Ждать исполнения входа"), PropertyOrder(11)]
        public int WaitLimitEnter { get; set; }
        [CategoryAttribute("Исполнение агента"), DisplayName("Имит. очередность позиций"), PropertyOrder(12)]
        public bool PriorityDeal { get; set; }
        [CategoryAttribute("Выставление заявок"), DisplayName("Проскальз. в шагах"), PropertyOrder(1)]
        public int SlippingInSteps { get; set; }
        [CategoryAttribute("Выставление заявок"), DisplayName("Проскальз. в %"), PropertyOrder(2)]
        public int SlippingInPercent { get; set; }
        [CategoryAttribute("Выставление заявок"), DisplayName("Take profit без проскальзывания"), PropertyOrder(3)]
        public bool TakeProfitNoSlip { get; set; }
        [CategoryAttribute("Выставление заявок"), DisplayName("Открытие лимитными заявками"), PropertyOrder(4)]
        public bool LimitDeal { get; set; }
        [CategoryAttribute("Выставление заявок"), DisplayName("\"По рынку\" с фикс. ценой"), PropertyOrder(5)]
        public bool MarketLimitPrice { get; set; }
        [CategoryAttribute("Выставление заявок"), DisplayName("\"Плохие\" заявки по рынку"), PropertyOrder(6)]
        public bool BadDeal 
        {
            get { return true; }
            set { BadDeal = value; }
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
            var x = PropertyGridControl;
            var res= x.SelectedObject;
            Close();
        }
    }
}
