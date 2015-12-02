using System.Windows;

namespace AistTrader
{
    public partial class AgentConfigView
    {

        public AgentConfigView()
        {
            InitializeComponent();
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            AddConfigWindow.Close();
        }

    }
}
