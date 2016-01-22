using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Common.Entities;
using Common.Settings;
using Ecng.Common;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    /// TODO: как делать рефреш rows?
    public partial class AgentGroupAddition
    {
        public int RowSetter;
        public ObservableCollection<Agent> AgentsStorage { get; private set; }
        public Agent AgentItem;
        public string OldGroupName;
        private int EditIndex { get; set; }
        public bool IsEditMode;
        public bool IsEnabledConfigBtn;
        public AgentWorkMode WorkMode;
        public int ItemCounter;
        public List<Agent> ItemsToDelete;
        public AgentGroupAddition()
        {
            InitializeComponent();
            EditIndex = int.MinValue;
            WorkMode = AgentWorkMode.Group;
            AgentsStorage = new ObservableCollection<Agent>();
            LoadSettings();
        }

        public AgentGroupAddition(Agent agent, int editIndex, AgentWorkMode editMode)
        {
            AgentsStorage = new ObservableCollection<Agent>();
            AgentItem = agent;
            InitializeComponent();
            LoadSettings();
            IsEditMode = true;
            IsEnabledConfigBtn = true;
            WorkMode = editMode;
            InitFields(agent, editMode);
            EditIndex = editIndex;
        }

        private void InitFields(Agent agent, AgentWorkMode editMode)
        {
            if (editMode == AgentWorkMode.Group)
            {
                OldGroupName = agent._Agent.GroupName;
                var itemsToEdit = AgentsStorage.Where(i => i._Agent.GroupName == agent._Agent.GroupName).Select(i => i).ToList();
                ItemCounter = itemsToEdit.Count;
                GroupNameTxtBox.IsEnabled = true;
                foreach (var i in itemsToEdit)
                {
                    AgentItem = i;
                    AddCongfigurationDuringGroupEdit(editMode);
                }
            }
            else if (editMode == AgentWorkMode.Single)
            {
                AgentItem = agent;
                AddCongfigurationDuringGroupEdit(editMode);
                AddConfigBtn.IsEnabled = false;
                GroupNameTxtBox.IsEnabled = false;
            }
        }

        private void AddCongfigurationDuringGroupEdit(AgentWorkMode agentEditMode)
        {
            if (agentEditMode == AgentWorkMode.Single)
            {
                RowSetter = 0;
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                var cb = new ComboBox
                {
                    Height = 23,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 180,
                    Margin = new Thickness {Left = 10, Top = 5, Right = 0, Bottom = 0},
                    ItemsSource = AgentsStorage.Where(i => i._Agent.GroupName == "Without Group").Select(i => i.Name),
                    SelectedItem = AgentItem.Name,
                    Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter),
                    IsEnabled = false,
                };
                cb.SelectionChanged += cb_SelectionChanged;
                var amount = new UnitEditor
                {
                    FontSize = 12,
                    Height = 23,
                    Width = 40,
                    Margin = new Thickness { Left = 0, Top = 5, Right = 0, Bottom = 0 },
                    Name = string.Format("{0}_{1}", "AmountTextBox", RowSetter)
                };

                if (IsEditMode && !AgentItem._Agent.Amount.IsNull())
                    amount.Text = AgentItem._Agent.Amount.ToString();
                if (IsEditMode && !AgentItem._Agent.GroupName.IsEmpty())
                    GroupNameTxtBox.Text = AgentItem._Agent.GroupName;
                var addDelControl = new Label
                {
                    Foreground = Brushes.Red,
                    Margin = new Thickness { Left = -10, Top = -4, Right = 29, Bottom = 0 },
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 20,
                    Height = 32,
                    Width = 27,
                    FontWeight = FontWeights.Bold,
                    Name = string.Format("{0}_{1}", "AddDelLabel", RowSetter)
                };
                if (IsEditMode)
                    AddConfigBtn.IsEnabled = IsEnabledConfigBtn;
                CreateGroupeBtn.Content = "Сохранить";
                DynamicGrid.RegisterName(amount.Name, amount);

                Grid.SetRow(cb, RowSetter);
                Grid.SetColumn(cb, 0);

                Grid.SetRow(amount, RowSetter);
                Grid.SetColumn(amount, 1);

                Grid.SetRow(addDelControl, RowSetter);
                Grid.SetColumn(addDelControl, 2);

                RowSetter++;
                DynamicGrid.Children.Add(cb);
                DynamicGrid.Children.Add(amount);
                DynamicGrid.Children.Add(addDelControl);
                CreateGroupBtnHelper();
            }

            if (agentEditMode == AgentWorkMode.Group)
            {
                if (DynamicGrid.ColumnDefinitions.Count == 0)
                {
                    RowSetter = 0;
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                }
                else
                    DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                var cb = new ComboBox
                {
                    Height = 23,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 180,
                    Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                    //TODO: дополнить условие выбора.важно после выбора алгоритма
                    ItemsSource = AgentsStorage.Where(i => i._Agent.GroupName == "Without Group").Select(i => i.Name),
                    SelectedItem = AgentItem.Name,
                    Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter)
                };
                cb.SelectionChanged += cb_SelectionChanged;

                var amount = new UnitEditor
                {
                    FontSize = 12,
                    Height = 23,
                    Width = 40,
                    Margin = new Thickness { Left = 0, Top = 5, Right = 0, Bottom = 0 },
                    Name = string.Format("{0}_{1}", "AmountTextBox", RowSetter)

                };
                if (IsEditMode && !AgentItem._Agent.Amount.IsNull())
                    amount.Text = AgentItem._Agent.Amount.ToString();
                if (IsEditMode && !AgentItem._Agent.GroupName.IsEmpty())
                    GroupNameTxtBox.Text = AgentItem._Agent.GroupName;
                var addDelControl = new Label
                {
                    Foreground = Brushes.Red,
                    Margin = new Thickness { Left = -10, Top = -4, Right = 29, Bottom = 0 },
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 20,
                    Height = 32,
                    Width = 27,
                    FontWeight = FontWeights.Bold,
                    Name = string.Format("{0}_{1}", "AddDelLabel", RowSetter)
                };
                DynamicGrid.RegisterName(addDelControl.Name, addDelControl);
                if (RowSetter > 0)
                    addDelControl.Content = "x";
                if (IsEditMode)
                    AddConfigBtn.IsEnabled = IsEnabledConfigBtn;
                CreateGroupeBtn.Content = "Сохранить";
                addDelControl.MouseDown += DelDynamicGridControl_MouseDown;
                DynamicGrid.RegisterName(amount.Name, amount);

                Grid.SetRow(cb, RowSetter);
                Grid.SetColumn(cb, 0);

                Grid.SetRow(amount, RowSetter);
                Grid.SetColumn(amount, 1);

                Grid.SetRow(addDelControl, RowSetter);
                Grid.SetColumn(addDelControl, 2);

                RowSetter++;
                DynamicGrid.Children.Add(cb);
                DynamicGrid.Children.Add(amount);
                DynamicGrid.Children.Add(addDelControl);
                CreateGroupBtnHelper();
            }
        }
        //TODO: check that method
        private void AddConfigBtnClick(object sender, RoutedEventArgs e)
        {
            if (DynamicGrid.ColumnDefinitions.Count == 0)
            {
                RowSetter = 0;
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            }
            else
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            var cb = new ComboBox
            {
                Height = 23,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 180,
                Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                //TODO: дополнить условие выбора.важно после выбора алгоритма
                ItemsSource = AgentsStorage.Where(i => i._Agent.GroupName == "Without Group").Select(i => i.Name),//NOTE: used for tests-> HelperStrategies.GetStrategies().Select(type => type.Name).ToList(),
                Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter)
            };
            cb.SelectionChanged += cb_SelectionChanged;


            var x = new UnitEditor();

            var amount = new UnitEditor
            {
                FontSize = 12,
                Height = 23,
                Width = 40,
                Margin = new Thickness { Left = 0, Top = 5, Right = 0, Bottom = 0 },
                Name = string.Format("{0}_{1}", "AmountTextBox", RowSetter)

            };
            if (IsEditMode && !AgentItem._Agent.GroupName.IsEmpty())
                GroupNameTxtBox.Text = AgentItem._Agent.GroupName;
            var addDelControl = new Label
            {
                Foreground = Brushes.Red,
                Margin = new Thickness { Left = -10, Top = -4, Right = 29, Bottom = 0 },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 20,
                Height = 32,
                Width = 27,
                FontWeight = FontWeights.Bold,
                Name = string.Format("{0}_{1}", "AddDelLabel", RowSetter)
            };
            DynamicGrid.RegisterName(addDelControl.Name, addDelControl);
            if (RowSetter > 0)
                addDelControl.Content = "x";
            if (IsEditMode)
                AddConfigBtn.IsEnabled = IsEnabledConfigBtn;
            CreateGroupeBtn.Content = "Сохранить";
            addDelControl.MouseDown += DelDynamicGridControl_MouseDown;
            DynamicGrid.RegisterName(amount.Name, amount);

            Grid.SetRow(cb, RowSetter);
            Grid.SetColumn(cb, 0);

            Grid.SetRow(amount, RowSetter);
            Grid.SetColumn(amount, 1);

            Grid.SetRow(addDelControl, RowSetter);
            Grid.SetColumn(addDelControl, 2);

            RowSetter++;
            DynamicGrid.Children.Add(cb);
            DynamicGrid.Children.Add(amount);
            DynamicGrid.Children.Add(addDelControl);
            CreateGroupBtnHelper();
        }

        void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int count = 0;
            var cbox = sender as ComboBox;
            if (cbox.SelectedItem != null)
            {
                foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>().Where(c => c.Name != cbox.Name && c.SelectedItem != null))
                {
                    if (cb.SelectedItem.ToString() == cbox.SelectedItem.ToString())
                        count++;
                    if (count == 1)
                    {
                        MessageBox.Show("Нельзя добавить одиновый алгоритм!");
                        cbox.SelectedIndex = -1;
                        break;
                    }
                }
            }
        }
        void DelDynamicGridControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WorkMode == AgentWorkMode.Group)
            {
                //TODO: определить необходимость удаления роу с грида для корректной прорисовки
                var item = (Label)e.Source;
                var cb = DynamicGrid.Children.OfType<ComboBox>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                var tb = DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                DynamicGrid.Children.Remove(cb);
                DynamicGrid.Children.Remove(tb);
                var label = DynamicGrid.Children.OfType<Label>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                DynamicGrid.Children.Remove(label);
            }
            else
            {
                var item = (Label)e.Source;
                var cb = DynamicGrid.Children.OfType<ComboBox>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                DynamicGrid.Children.Remove(cb);
                var tb = DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                DynamicGrid.Children.Remove(tb);
                var label = DynamicGrid.Children.OfType<Label>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                DynamicGrid.Children.Remove(label);
            }
        }

        private void CreateGroupBtnClick(object sender, RoutedEventArgs e)
        {
            var gridItems = DynamicGrid.Children.Cast<UIElement>().Where(i => Grid.GetRow(i) == 0);
            foreach (var i in gridItems)
            {
                if (DynamicGrid.Children.OfType<ComboBox>().Any(cb => cb.SelectedIndex == -1))
                {
                    MessageBox.Show("Не выбран алгоритм");
                    return;
                }
                if (DynamicGrid.Children.OfType<UnitEditor>().Any(cb => cb.Text == ""))
                {
                    MessageBox.Show("Не все объемы заполненны");
                    return;
                }
                if (GroupNameTxtBox.Text == "")
                {
                    MessageBox.Show("Задать имя группы");
                    return;
                }
            }
            var unitItems = DynamicGrid.Children.OfType<UnitEditor>().Select(i => i).ToList();
            foreach (var i in unitItems)
            {
                try
                {
                    //i.Value = i.Text.ToUnit();
                }
                catch (Exception)
                {
                    MessageBox.Show("Задано некорректное значение объема: {0}".Put(i.Text));
                    return;
                }
            }

            if (WorkMode == AgentWorkMode.Group)
            {
                foreach (var item in AgentsStorage.Where(a => a._Agent.GroupName == OldGroupName/*GroupNameTxtBox.Text*/).ToList())
                {
                    MainWindow.Instance.DelAgentConfigBtnClick(item);
                }
                foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                {
                    string cbID = cb.Name.Split('_').Last();
                    if (cbID != "")
                        foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                        {
                            Unit amount = ue.Text.ToUnit();
                            string algorithmName = cb.Text;
                            var groupName = GroupNameTxtBox.Text;
                            foreach (var rs in AgentsStorage.Where(a => a.Name == algorithmName && a._Agent.GroupName == "Without Group"))
                            {
                                var newAgent = (Agent)rs.Clone();
                                newAgent._Agent.Amount = amount.Value;
                                newAgent._Agent.GroupName = groupName;
                                MainWindow.Instance.AddNewAgent(newAgent, -1);
                            }
                        }
                }
            }
            else
            {
                foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                {
                    string cbID = cb.Name.Split('_').Last();
                    if (cbID != "")
                        foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                        {
                            //Unit amount = ue.Value;
                            string algorithmName = cb.Text;
                            var groupName = GroupNameTxtBox.Text;
                            var items = AgentsStorage.Where(a => a.Name == algorithmName && a._Agent.GroupName == groupName).ToList();
                            int itemIndex = -1;
                            foreach (var i in items)
                                itemIndex = AgentsStorage.IndexOf(i);
                            var itemToEdit = AgentsStorage[itemIndex];
                            //itemToEdit._Agent.Amount = amount;
                            itemToEdit._Agent.GroupName = groupName;
                            itemToEdit.Name = algorithmName;
                            MainWindow.Instance.AddNewAgent(itemToEdit, EditIndex);
                        }
                }
            }
            Close();
        }
        private void StratSettings_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsEditMode)
                AddConfigBtnClick(sender, e);
            CreateGroupBtnHelper();
        }
        private void CreateGroupBtnHelper()
        {
            if (DynamicGrid.Children.Count == 0)
                CreateGroupeBtn.IsEnabled = false;
            else
                CreateGroupeBtn.IsEnabled = true;
        }
        private void LoadSettings()
        {
            StreamReader sr = new StreamReader("AgentSettings.xml");
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<Agent>), new Type[] { typeof(Agent) });
                var connections = (List<Agent>)xmlSerializer.Deserialize(sr);
                sr.Close();
                foreach (var rs in connections.Cast<Agent>())
                {
                    AgentsStorage.Add(rs);
                }
            }
            catch (Exception e)
            {
                sr.Close();
                //if (e.InnerException.Message == "Root element is missing.")
                //    try
                //    {
                //        System.IO.File.WriteAllText("AgentSettings.xml", string.Empty);
                //    }
                //    catch (Exception)
                //    {
                //    }
            }
        }
    }
}
