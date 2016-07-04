using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Common.Entities;
using Common.Params;
using DevExpress.Xpf.Grid.Printing;
using Ecng.Common;
using StockSharp.Messages;
using StockSharp.Xaml;

namespace AistTrader
{
    /// TODO: как делать рефреш rows?
    public partial class GroupAddition
    {
        //TODO: добавитть верификацию и реджекс на ввод данных в динамических полях


        public int RowSetter;
        // public ObservableCollection<Agent> AgentsStorage { get; private set; }
        public Agent AgentItem;
        public string OldGroupName;
        private int EditIndex { get; set; }
        public bool IsEditMode;
        public bool IsEnabledConfigBtn;
        public AgentWorkMode WorkMode;
        public int ItemCounter;
        public List<Agent> ItemsToDelete;
        public GroupAddition()
        {
            InitializeComponent();
            EditIndex = int.MinValue;
            WorkMode = AgentWorkMode.Group;
            //AgentsStorage = new ObservableCollection<Agent>();
            //LoadSettings();
        }

        public GroupAddition(Agent agent, int editIndex, AgentWorkMode editMode)
        {
            //AgentsStorage = new ObservableCollection<Agent>();
            AgentItem = agent;
            InitializeComponent();
            //LoadSettings();
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
                OldGroupName = agent.Params.GroupName;
                var itemsToEdit = MainWindow.Instance.AgentsStorage.Where(i => i.Params.GroupName == agent.Params.GroupName).Select(i => i).ToList();
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
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(290) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
                var cb = new ComboBox
                {
                    Height = 28,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 280,
                    Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                    ItemsSource = MainWindow.Instance.AgentsStorage.Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName),
                    SelectedItem = AgentItem.Params.FriendlyName,
                    Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter),
                    IsEnabled = false,
                };
                cb.SelectionChanged += cb_SelectionChanged;
                var amount = new UnitEditor
                {
                    FontSize = 12,
                    Height = 28,
                    Width = 40,
                    Margin = new Thickness { Left = 0, Top = 5, Right = 0, Bottom = 0 },
                    Name = string.Format("{0}_{1}", "AmountTextBox", RowSetter)
                };
                amount.KeyUp += Amount_KeyUp;

                if (IsEditMode && !AgentItem.Params.Amount.IsNull())
                    amount.Text = AgentItem.Params.Amount.ToString();
                if (IsEditMode && !AgentItem.Params.GroupName.IsEmpty())
                    GroupNameTxtBox.Text = AgentItem.Params.GroupName;
                var addDelControl = new Label
                {
                    Foreground = Brushes.Red,
                    Margin = new Thickness { Left = -10, Top = 0, Right = 29, Bottom = 0 },
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
                CreateGroupeBtn.Content = "Save";
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
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(290) });
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
                }
                else
                    DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
                var cb = new ComboBox
                {
                    Height = 28,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 280,
                    Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                    //TODO: дополнить условие выбора.важно после выбора алгоритма
                    ItemsSource = MainWindow.Instance.AgentsStorage.Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName),
                    SelectedItem = AgentItem.Params.FriendlyName,
                    Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter)
                };
                cb.SelectionChanged += cb_SelectionChanged;

                var amount = new UnitEditor
                {
                    FontSize = 12,
                    Height = 28,
                    Width = 40,
                    Margin = new Thickness { Left = 0, Top = 5, Right = 0, Bottom = 0 },
                    Name = string.Format("{0}_{1}", "AmountTextBox", RowSetter)

                };
                amount.KeyUp += Amount_KeyUp;
                if (IsEditMode && !AgentItem.Params.Amount.IsNull())
                    amount.Text = AgentItem.Params.Amount.ToString();
                if (IsEditMode && !AgentItem.Params.GroupName.IsEmpty())
                    GroupNameTxtBox.Text = AgentItem.Params.GroupName;
                var addDelControl = new Label
                {
                    Foreground = Brushes.Red,
                    Margin = new Thickness { Left = -10, Top = 0, Right = 0, Bottom = 0 },
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
                CreateGroupeBtn.Content = "Save";
                if (RowSetter != 0)
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
        private void Amount_KeyUp(object sender, KeyEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            var editor = sender as UnitEditor;
            if (editor.Text.EndsWith("%"))
            {
                string[] line = editor.Text.Split('%');
                if (!regex.IsMatch(line.First()))
                {
                    //MessageBox.Show("Возможет ввод только цифр или цифры со знаком % на конце");
                    editor.Text = "";
                    return;
                    // editor.Select(editor.Text.Length, 0);
                }
            }
            if (!editor.Text.EndsWith("%"))
            {
                if (!regex.IsMatch(editor.Text))
                {
                    //MessageBox.Show("Возможет ввод только цифр или цифры со знаком % на конце");
                    editor.Text = "";
                    return;
                    //editor.Select(editor.Text.Length, 0);
                }
            }
        }
        //TODO: check that method
        private void AddConfigBtnClick(object sender, RoutedEventArgs e)
        {
            if (DynamicGrid.ColumnDefinitions.Count == 0)
            {
                RowSetter = 0;
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(290) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
            }
            else
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
            var cb = new ComboBox
            {
                Height = 28,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 280,
                Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                //TODO: дополнить условие выбора.важно после выбора алгоритма
                ItemsSource = MainWindow.Instance.AgentsStorage.Where(i => i.Params.GroupName == "ungrouped agents").Select(i => i.Params.FriendlyName),//NOTE: used for tests-> HelperStrategies.GetStrategies().Select(type => type.Name).ToList(),
                Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter)
            };
            cb.SelectionChanged += cb_SelectionChanged;
            var amount = new UnitEditor
            {
                FontSize = 12,
                Height = 28,
                Width = 40,
                Margin = new Thickness { Left = -5, Top = 5, Right = 0, Bottom = 0 },
                Name = string.Format("{0}_{1}", "AmountTextBox", RowSetter)
            };
            amount.KeyUp += Amount_KeyUp; ;
            if (IsEditMode && !AgentItem.Params.GroupName.IsEmpty())
                GroupNameTxtBox.Text = AgentItem.Params.GroupName;
            var addDelControl = new Label
            {
                Foreground = Brushes.Red,
                Margin = new Thickness { Left = -10, Top = 0, Right = 0, Bottom = 0 },
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
            CreateGroupeBtn.Content = "Save";
            if (RowSetter != 0)
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
                        MessageBox.Show("Can not add the same script!");
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
                    MessageBox.Show("Scrtipt is not set");
                    return;
                }
                if (DynamicGrid.Children.OfType<UnitEditor>().Any(cb => cb.Text == ""))
                {
                    MessageBox.Show("Not all amounts have been set");
                    return;
                }
                if (GroupNameTxtBox.Text == "")
                {
                    MessageBox.Show("Group name is not set");
                    return;
                }
            }

            //todo: поменять логику, просто удалять то что нельзя вводить
            var unitItems = DynamicGrid.Children.OfType<UnitEditor>().Select(i => i).ToList();
            if (!IsEditMode && WorkMode== AgentWorkMode.Single && unitItems.Count==1)
            {
                MessageBox.Show("Group can not be composed with 1 script");
                return;
            }
            if (IsEditMode && WorkMode == AgentWorkMode.Group && unitItems.Count == 1)
            {
                MessageBox.Show("Group can not be composed with 1 script");
                return;
            }
            foreach (var i in unitItems)
            {
                try
                {
                    //i.Value = i.Text.ToUnit();
                }
                catch (Exception)
                {
                    MessageBox.Show("Incorrect amount value : {0}".Put(i.Text));
                    return;
                }
            }

            if (IsEditMode)
             {
                if (WorkMode == AgentWorkMode.Group)
                {
                    var oldItems =MainWindow.Instance.AgentsStorage.Where(a => a.Params.GroupName == OldGroupName).ToList();
                    List<Agent> newMembersOfCurrentGroup = new List<Agent>();

                    foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                    {
                        string cbID = cb.Name.Split('_').Last();
                        if (cbID != "")
                            foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                            {
                                var newItems = DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)).ToList();
                                var amount = ue.Text;
                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                var isInCollection = oldItems.Any(i => i.Name == algorithmName);

                                if (oldItems.Any(i => i.Name == algorithmName))
                                {
                                    var item = MainWindow.Instance.AgentsStorage.FirstOrDefault(i => i.Name== algorithmName &&  i.Params.GroupName == OldGroupName);
                                    item.Params.PhantomParams.AgentName = item.Name;
                                    item.Params.PhantomParams.Amount = item.Params.Amount;
                                    item.Params.PhantomParams.GroupName = item.Params.GroupName;

                                    item.Params.Amount = amount;
                                    item.Params.GroupName = groupName;
                                    var index = MainWindow.Instance.AgentsStorage.IndexOf(item);
                                    MainWindow.Instance.AddNewAgentInGroup(item, index, false);
                                    //go to agent manager related actions
                                    var amItemOnTheFly = MainWindow.Instance.AgentManagerStorage.Where(i => i.AgentManagerSettings.AgentOrGroup == item.Params.GroupName.ToString()).ToList();
                                    foreach (var amItem in amItemOnTheFly)
                                    {
                                        if (amItem != null && amItem.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Running)
                                        {
                                            var runnigStrategy = MainWindow.Instance.AgentConnnectionManager.FirstOrDefault(i => i.ActualStrategyRunning.Name.EndsWith(item.Name));
                                            if (runnigStrategy != null)
                                            {
                                                var ueAmount = new UnitEditor();
                                                ueAmount.Text = item.Params.Amount;
                                                ueAmount.Value = ueAmount.Text.ToUnit();
                                                decimal? calculatedAmount = 0;
                                                if (ueAmount.Value.Type == UnitTypes.Percent)
                                                {
                                                    calculatedAmount = MainWindow.Instance.CalculateAmount(amItem, item);
                                                    runnigStrategy.ActualStrategyRunning.Volume = /*Convert.ToDecimal(itemToEdit.Params.Amount)*/ (decimal)calculatedAmount;
                                                }
                                                if (ueAmount.Value.Type == UnitTypes.Absolute)
                                                {
                                                    calculatedAmount = ueAmount.Value.To<decimal>();
                                                    runnigStrategy.ActualStrategyRunning.Volume =(decimal)calculatedAmount;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //offline
                                        }

                                    }
                                    newMembersOfCurrentGroup.Add(item);
                                }
                                else
                                {
                                    List<Agent> list = new List<Agent>();
                                    foreach (var rs in MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == "ungrouped agents"))
                                    {
                                        var newAgent = (Agent)rs.Clone();
                                        newAgent.Params.AgentCompiledName = rs.Params.AgentCompiledName;
                                        newAgent.Params.Amount = amount;
                                        newAgent.Params.GroupName = groupName;
                                        newAgent.Params.ToolTipName = rs.Params.ToolTipName;
                                        list.Add(newAgent);
                                    }
                                    foreach (var i in list)
                                    {
                                        MainWindow.Instance.AddNewAgentInGroup(i, -1, true);
                                        newMembersOfCurrentGroup.Add(i);
                                    }
                                }
                            }
                    }
                    foreach (var oldItem in oldItems)
                    {
                        var IsItemToDelete = newMembersOfCurrentGroup.All(i => i != oldItem);
                        if (IsItemToDelete)
                            MainWindow.Instance.DelAgentConfigBtnClick(oldItem, "has been excluded from the group");

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
                                var amount = ue.Text;
                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                var items = MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == groupName).ToList();

                                int itemIndex = -1;
                                foreach (var i in items)
                                    itemIndex = MainWindow.Instance.AgentsStorage.IndexOf(i);
                                var itemToEdit = MainWindow.Instance.AgentsStorage[itemIndex];

                                itemToEdit.Params.PhantomParams.AgentName = itemToEdit.Name;
                                itemToEdit.Params.PhantomParams.Amount = itemToEdit.Params.Amount;
                                //itemToEdit.Params.PhantomParams.GroupName = itemToEdit.Params.GroupName;

                                itemToEdit.Params.Amount = amount;
                                itemToEdit.Params.GroupName = groupName;
                                itemToEdit.Name = algorithmName;
                                MainWindow.Instance.AddNewAgent(itemToEdit, EditIndex);
                                //go to agent manager related actions
                                var amItemOnTheFly = MainWindow.Instance.AgentManagerStorage.Where(i => i.AgentManagerSettings.AgentOrGroup == itemToEdit.Params.GroupName.ToString()).ToList();
                                foreach (var amItem in amItemOnTheFly)
                                {
                                    if (amItem != null && amItem.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Running)
                                    {
                                        var runnigStrategy = MainWindow.Instance.AgentConnnectionManager.FirstOrDefault(i => i.ActualStrategyRunning.Name.EndsWith(itemToEdit.Name));
                                        if (runnigStrategy != null)
                                        {
                                            var ueAmount = new UnitEditor();
                                            ueAmount.Text = itemToEdit.Params.Amount;
                                            ueAmount.Value = ueAmount.Text.ToUnit();
                                            decimal? calculatedAmount = 0;
                                            if (ueAmount.Value.Type == UnitTypes.Percent)
                                            {
                                                calculatedAmount = MainWindow.Instance.CalculateAmount(amItem, itemToEdit);
                                                runnigStrategy.ActualStrategyRunning.Volume = /*Convert.ToDecimal(itemToEdit.Params.Amount)*/ (decimal)calculatedAmount;
                                            }
                                            if (ueAmount.Value.Type == UnitTypes.Absolute)
                                            {
                                                calculatedAmount = ueAmount.Value.To<decimal>();
                                                runnigStrategy.ActualStrategyRunning.Volume = (decimal)calculatedAmount;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //offline
                                    }

                                }
                                
                            }
                    }
                }
                Close();
            }

            else
            {
                if (WorkMode == AgentWorkMode.Group)
                {
                    foreach (var item in MainWindow.Instance.AgentsStorage.Where(a => a.Params.GroupName == OldGroupName/*GroupNameTxtBox.Text*/).ToList())
                    {
                        MainWindow.Instance.DelAgentConfigBtnClick(item,null);
                    }
                    foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                    {
                        string cbID = cb.Name.Split('_').Last();
                        if (cbID != "")
                            foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                            {

                                var amount = ue.Text;

                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                List<Agent> list = new List<Agent>();
                                foreach (var rs in MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == "ungrouped agents"))
                                {
                                    var newAgent = (Agent)rs.Clone();
                                    newAgent.Params.AgentCompiledName = rs.Params.AgentCompiledName;
                                    newAgent.Params.Amount = amount;
                                    newAgent.Params.GroupName = groupName;
                                    newAgent.Params.ToolTipName = rs.Params.ToolTipName;
                                    list.Add(newAgent);
                                    var index = MainWindow.Instance.AgentsStorage.IndexOf(rs);
                                    //MainWindow.Instance.AddNewAgent(newAgent, -1);
                                }
                                foreach (var i in list)
                                {
                                    MainWindow.Instance.AddNewAgentInGroup(i, -1, false);
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
                                var amount = ue.Text;
                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                var items = MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == groupName).ToList();
                                int itemIndex = -1;
                                foreach (var i in items)
                                    itemIndex = MainWindow.Instance.AgentsStorage.IndexOf(i);
                                var itemToEdit = MainWindow.Instance.AgentsStorage[itemIndex];
                                itemToEdit.Params.Amount = amount;
                                itemToEdit.Params.GroupName = groupName;
                                itemToEdit.Name = algorithmName;
                                MainWindow.Instance.AddNewAgent(itemToEdit, EditIndex);
                            }
                    }
                }
                Close();
            }
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
    }
}
