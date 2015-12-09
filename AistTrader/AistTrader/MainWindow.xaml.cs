using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AistTrader.Properties;
using Common.Entities;
using MoreLinq;

namespace AistTrader
{
    public partial class MainWindow
    {

        public static MainWindow Instance { get; private set; }
        //public List<Security> SecuritiesList { get; set; } 

        public MainWindow()
        {


            //Agents



            //Settings.Default.AistTrader.Clear();
            //Settings.Default.AgentConnection.Clear();
            //Settings.Default.AgentManager.Clear();
            //Settings.Default.AgentPortfolio.Clear();
            //Settings.Default.Agents.Clear();











            Instance = this;
            var x = Settings.Default;
            SetConnectionValuesToDefault();

        }


        //*Shit hits the fan time coding bitch

        private void TabCtr_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.OriginalSource is TabControl && AgentItem != null && AgentItem.IsSelected)
            {
                LoadAgentTabItemData();
            }

            if (e.OriginalSource is TabControl && ProviderItem != null && ProviderItem.IsSelected)
            {
                LoadProviderTabItemData();
            }
            if (e.OriginalSource is TabControl && PortfolioItem != null && PortfolioItem.IsSelected)
            {
                LoadPortfolioTabItemData();
            }
            if (e.OriginalSource is TabControl && AgentManagerItem != null && AgentManagerItem.IsSelected)
            {
                LoadAgentManagerTabItemData();
            }


        }



















        //public static void Unique()
        //{
        //    bool isUnique = SecurityList.Distinct().Count() == SecurityList.Count();
        //}

        private static void UpdateWorkingDays()
        {
            // Заполнено в библиотеке на 2011-2013 года
            //var specialWorkingDays = new[]
            //						 {
            //							 new DateTime(2012, 12, 29)
            //						 };
            //var specialHolidays = new[]
            //					  {
            //						  new DateTime(2012, 12, 31)
            //					  };

            //ExchangeBoard.Forts.WorkingTime.SpecialWorkingDays = specialWorkingDays;
            //ExchangeBoard.Forts.WorkingTime.SpecialHolidays = specialHolidays;

            //ExchangeBoard.Micex.WorkingTime.SpecialWorkingDays = specialWorkingDays;
            //ExchangeBoard.Micex.WorkingTime.SpecialHolidays = specialHolidays;
        }



        private void SetConnectionValuesToDefault()
        {
            //ToDo:обнуление на неактивное

            if (Settings.Default.AgentConnection != null)
                Settings.Default.AgentConnection.Cast<AgentConnection>().ForEach(i => i.Connection.IsActive = false);
            Settings.Default.Save();

        }


        private void SetStartBtnStatus()
        {
            //StartBtn.IsEnabled = Settings.Default.Robots != null && Settings.Default.Robots.Count > 0 &&
            //    Settings.Default.Robots.Cast<Account>().Any(r => r.Agent.IsEnabled);
            //StartBtn.IsEnabled = true;

        }




        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        //TODO: подключить в последствии..
        private void AgentAddConfigMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = AgentItem;

            //MainFrame.Navigate(new AgentManagerForm());
            //new AgentForm().ShowDialog();



            //var form = new AgentForm();
            //form.ShowDialog();
            //form.Close();
            //form = null;
        }
        private void AgentManagerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //new AgentManagerForm().ShowDialog();
            //var form = new AgentManagerForm();
            //form.ShowDialog();
            //form.Close();
            //form = null;
            TabCtr.SelectedItem = AgentManagerItem;
        }
        private void ProviderManagerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = ProviderItem;

            //new ProviderManagerForm().ShowDialog();
        }


        private void PortfolioMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TabCtr.SelectedItem = PortfolioItem;
        }
        
        private void WhatsNewItem_OnClick(object sender, RoutedEventArgs e)
        {
            
            //открытие окна, где можно посмотреть, что нового
            var form = new WhatsNew().ShowDialog();

        }
    }
}
