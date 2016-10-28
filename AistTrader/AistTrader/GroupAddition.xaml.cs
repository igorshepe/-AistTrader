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
using StockSharp.Algo;
using Strategies.Strategies;

namespace AistTrader
{
    /// TODO: как делать рефреш rows?
    public partial class GroupAddition
    {
        //TODO: добавитть верификацию и реджекс на ввод данных в динамических полях
        public int RowSetter;
        public Agent AgentItem;
        public string OldGroupName;
        private int EditIndex { get; set; }
        public bool IsEditMode;
        public bool IsEnabledConfigBtn;
        public AgentWorkMode WorkMode;
        public int ItemCounter;
        public List<Agent> ItemsToDelete;
        private int removeCount;
        private int editCount;
        private List<string> currentSecurities;

        public GroupAddition()
        {
            InitializeComponent();
            EditIndex = int.MinValue;
            WorkMode = AgentWorkMode.Group;
            currentSecurities = new List<string>();
        }

        public GroupAddition(Agent agent, int editIndex, AgentWorkMode editMode)
        {
            AgentItem = agent;
            InitializeComponent();
            IsEditMode = true;
            IsEnabledConfigBtn = true;
            WorkMode = editMode;
            InitFields(agent, editMode);
            EditIndex = editIndex;
            currentSecurities = new List<string>();
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
                SetSelectedInstruments(itemsToEdit);
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
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(360) });
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
                {
                    amount.Text = AgentItem.Params.Amount.ToString();
                }
                if (IsEditMode && !AgentItem.Params.GroupName.IsEmpty())
                {
                    GroupNameTxtBox.Text = AgentItem.Params.GroupName;
                }
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
                {
                    AddConfigBtn.IsEnabled = IsEnabledConfigBtn;
                }
                CreateGroupeBtn.Content = "Save";
                DynamicGrid.RegisterName(amount.Name, amount);

                Grid.SetRow(cb, RowSetter);
                Grid.SetColumn(cb, 0);

                Grid.SetRow(amount, RowSetter);
                Grid.SetColumn(amount, 1);

                Grid.SetRow(addDelControl, RowSetter);
                Grid.SetColumn(addDelControl, 2);

                ++RowSetter;
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
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(360) });
                    DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
                }
                else
                {
                    DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
                }
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

                List<StockSharp.BusinessEntities.Security> instruments = new List<StockSharp.BusinessEntities.Security>();
                MainWindow.Instance.ConnectionsStorage.Where(c => true).ToList().ForEach(c => instruments.AddRange(c.ConnectionParams.Tools ?? new List<StockSharp.BusinessEntities.Security>()));
                instruments = instruments.GroupBy(i => i.Id).Select(g => g.First()).ToList();

                var instrument = new SecurityEditor
                {
                    Height = 28,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 340,
                    Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                    SecurityProvider = new CollectionSecurityProvider(instruments),
                    Name = string.Format("{0}_{1}", "InstrumentComboBox", RowSetter)
                };
                instrument.SecuritySelected += new Action(() => instrument_SelectionChanged(instrument));
                DynamicGrid.RegisterName(instrument.Name, instrument);

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
                {
                    amount.Text = AgentItem.Params.Amount.ToString();
                }
                if (IsEditMode && !AgentItem.Params.GroupName.IsEmpty())
                {
                    GroupNameTxtBox.Text = AgentItem.Params.GroupName;
                }
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
                {
                    addDelControl.Content = "x";
                }
                if (IsEditMode)
                {
                    AddConfigBtn.IsEnabled = IsEnabledConfigBtn;
                }
                CreateGroupeBtn.Content = "Save";
                if (RowSetter != 0)
                {
                    addDelControl.MouseDown += DelDynamicGridControl_MouseDown;
                }
                DynamicGrid.RegisterName(amount.Name, amount);

                Grid.SetRow(cb, RowSetter);
                Grid.SetColumn(cb, 0);

                Grid.SetRow(instrument, RowSetter);
                Grid.SetColumn(instrument, 1);

                Grid.SetRow(amount, RowSetter);
                Grid.SetColumn(amount, 2);

                Grid.SetRow(addDelControl, RowSetter);
                Grid.SetColumn(addDelControl, 3);

                ++RowSetter;
                DynamicGrid.Children.Add(cb);
                DynamicGrid.Children.Add(amount);
                DynamicGrid.Children.Add(addDelControl);
                DynamicGrid.Children.Add(instrument);
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
                    editor.Text = string.Empty;
                    return;
                }
            }

            if (!editor.Text.EndsWith("%"))
            {
                if (!regex.IsMatch(editor.Text))
                {
                    editor.Text = string.Empty;
                    return;
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
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(360) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
            }
            else
            {
                DynamicGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(33) });
            }

            List<string> excluded = DynamicGrid.Children.OfType<ComboBox>().Where(c => c.SelectedItem != null).Select(c => c.Text).ToList();

            var cb = new ComboBox
            {
                Height = 28,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 280,
                Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                //TODO: дополнить условие выбора.важно после выбора алгоритма
                ItemsSource = MainWindow.Instance.AgentsStorage.Where(i => i.Params.GroupName == "ungrouped agents" && !excluded.Any(x => x == i.Name)).Select(i => i.Params.FriendlyName),//NOTE: used for tests-> HelperStrategies.GetStrategies().Select(type => type.Name).ToList(),
                Name = string.Format("{0}_{1}", "AlgorithmComboBox", RowSetter)
            };
            cb.SelectionChanged += cb_SelectionChanged;

            List<StockSharp.BusinessEntities.Security> instruments = new List<StockSharp.BusinessEntities.Security>();
            MainWindow.Instance.ConnectionsStorage.Where(c => true).ToList().ForEach(c => instruments.AddRange(c.ConnectionParams.Tools ?? new List<StockSharp.BusinessEntities.Security>()));
            instruments = instruments.GroupBy(i => i.Id).Select(g => g.First()).ToList();

            var instrument = new SecurityEditor
            {
                Height = 28,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 340,
                Margin = new Thickness { Left = 10, Top = 5, Right = 0, Bottom = 0 },
                SecurityProvider = new CollectionSecurityProvider(instruments),
                Name = string.Format("{0}_{1}", "InstrumentComboBox", RowSetter)
            };
            instrument.SecuritySelected += new Action(() => instrument_SelectionChanged(instrument));
            DynamicGrid.RegisterName(instrument.Name, instrument);

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
            {
                GroupNameTxtBox.Text = AgentItem.Params.GroupName;
            }
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
            {
                addDelControl.Content = "x";
            }
            if (IsEditMode)
            {
                AddConfigBtn.IsEnabled = IsEnabledConfigBtn;
            }
            CreateGroupeBtn.Content = "Save";
            if (RowSetter != 0)
            {
                addDelControl.MouseDown += DelDynamicGridControl_MouseDown;
            }
            DynamicGrid.RegisterName(amount.Name, amount);

            Grid.SetRow(cb, RowSetter);
            Grid.SetColumn(cb, 0);

            Grid.SetRow(instrument, RowSetter);
            Grid.SetColumn(instrument, 1);

            Grid.SetRow(amount, RowSetter);
            Grid.SetColumn(amount, 2);

            Grid.SetRow(addDelControl, RowSetter);
            Grid.SetColumn(addDelControl, 3);

            ++RowSetter;
            DynamicGrid.Children.Add(cb);
            DynamicGrid.Children.Add(amount);
            DynamicGrid.Children.Add(addDelControl);
            DynamicGrid.Children.Add(instrument);
            CreateGroupBtnHelper();
        }

        void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int count = 0;
            var cbox = sender as ComboBox;
            string currentTxt = (string)cbox.SelectedItem;
            if (cbox.SelectedItem != null)
            {
                List<string> excluded = DynamicGrid.Children.OfType<ComboBox>().Where(c => c.SelectedItem != null).Select(c => c.SelectedItem is string ? (string)c.SelectedItem : ((StockSharp.BusinessEntities.Security)c.SelectedItem).ShortName).ToList();

                foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>().Where(c => !c.Name.Contains("InstrumentComboBox")))
                {
                    string text = (string)cb.SelectedItem;
                    cb.ItemsSource = MainWindow.Instance.AgentsStorage.Where(i => i.Params.GroupName == "ungrouped agents"
                        && (!excluded.Any(x => x == i.Name) || i.Name == currentTxt && cb == cbox || i.Name == text && cb != cbox)).Select(i => i.Params.FriendlyName);
                    cb.SelectedIndex = cb.Items.IndexOf(text);
                }

                foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>().Where(c => c.Name != cbox.Name && c.SelectedItem != null && !c.Name.Contains("InstrumentComboBox")))
                {
                    if (cb.SelectedItem.ToString() == cbox.SelectedItem.ToString())
                    {
                        ++count;
                    }
                    if (count == 1)
                    {
                        MessageBox.Show("Can not add the same script!");
                        cbox.SelectedIndex = -1;
                        break;
                    }
                }
            }
        }

        private void SetSelectedInstruments(List<Agent> agents)
        {
            List<StockSharp.BusinessEntities.Security> instruments = new List<StockSharp.BusinessEntities.Security>();
            MainWindow.Instance.ConnectionsStorage.Where(c => true).ToList().ForEach(c => instruments.AddRange(c.ConnectionParams.Tools ?? new List<StockSharp.BusinessEntities.Security>()));
            int i = 0;
            foreach (SecurityEditor se in DynamicGrid.Children.OfType<SecurityEditor>().Where(c => c.Name.Contains("InstrumentComboBox")))
            {
                Agent agent = MainWindow.Instance.AgentsStorage.FirstOrDefault(a => a.Name == agents[i].Name && a.Params.GroupName == agents[i].Params.GroupName);
                se.Text = agent.Params.Security;
                ++i;
            }
        }

        private void ResetCurrentSecurities(int index, string security)
        {
            if (currentSecurities == null)
            {
                currentSecurities = new List<string>();
            }
            List<string> tmp = new List<string>(currentSecurities.Count);
            for (int i = 0, n = currentSecurities.Count; i < n; ++i)
            {
                tmp.Add(currentSecurities[i]);
            }
            if (index < currentSecurities.Count)
            {
                currentSecurities[index] = security;
            }
            else
            {
                currentSecurities = new List<string>(index + 1);
                for (int i = 0, n = tmp.Count; i < n; ++i)
                {
                    currentSecurities.Add(tmp[i]);
                }
                for (int i = tmp.Count; i < index + 1; ++i)
                {
                    currentSecurities.Add(string.Empty);
                }
                currentSecurities[index] = security;
            }
        }

        void instrument_SelectionChanged(object sender)
        {
            var item = (Control)sender;
            int index = (int)item.GetValue(Grid.RowProperty);
            SecurityEditor se = (SecurityEditor)sender;
            ResetCurrentSecurities(index, se.Text);
        }

        void DelDynamicGridControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var delItem = MainWindow.Instance.AgentsStorage.FirstOrDefault(a => ((Label)e.Source).Name == a.Name);

            if (WorkMode == AgentWorkMode.Group)
            {
                var item = (Label)e.Source;
                int index = (int)item.GetValue(Grid.RowProperty);

                var cb = DynamicGrid.Children.OfType<ComboBox>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                var tb = DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();

                var strategyName = (string)cb.SelectedValue;
                var agentManager = MainWindow.Instance.AgentManagerStorage.Where(am => am.StrategyInGroup.Any(s => s.Name == strategyName) && MainWindow.Instance.AgentsStorage.Any(a => a.Params.GroupName == am.Alias && a.Name == strategyName)).FirstOrDefault();
                bool doRequest = agentManager != null && agentManager.StrategyInGroup.Any(s => s.Position != 0);
                bool doDelete = true;
                if (doRequest)
                {
                    var agentToDelete =
                        MainWindow.Instance.AgentConnnectionManager.Strategies.FirstOrDefault(
                            it => it.ActualStrategyRunning.Name == item.Name);

                    var form = new GroupAdditionDeleteMode(item.Name.ToString());
                    form.ShowDialog();
                    var selectedMode = form.SelectedDeleteMode;
                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                    {
                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                        strat.CheckPosExit();
                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                        MainWindow.Instance.DelAgentConfigBtnClick(delItem, "has been excluded from the group");
                    }
                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                    {
                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                        strat.CheckPosWaitStrExit();
                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                        MainWindow.Instance.DelAgentConfigBtnClick(delItem, "has been excluded from the group");
                    }
                    doDelete = !form.IsCancelled;
                }
                if (doDelete)
                {
                    DynamicGrid.Children.Remove(cb);
                    DynamicGrid.Children.Remove(tb);
                    var label = DynamicGrid.Children.OfType<Label>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                    DynamicGrid.Children.Remove(label);

                    DynamicGrid.RowDefinitions[index].Height = new GridLength(0);
                }
            }
            else
            {
                var item = (Label)e.Source;
                var cb = DynamicGrid.Children.OfType<ComboBox>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                //DynamicGrid.Children.Remove(cb);
                var tb = DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                //DynamicGrid.Children.Remove(tb);
                var label = DynamicGrid.Children.OfType<Label>().Where(c => c.Name.EndsWith((item.Name.Split('_').Last()))).Select(c => c).First();
                //DynamicGrid.Children.Remove(label);

                var strategyName = (string)cb.SelectedValue;
                var agentManager = MainWindow.Instance.AgentManagerStorage.Where(am => am.StrategyInGroup.Any(s => s.Name == strategyName) && MainWindow.Instance.AgentsStorage.Any(a => a.Params.GroupName == am.Alias && a.Name == strategyName)).FirstOrDefault();
                bool doRequest = agentManager != null && agentManager.SingleAgentPosition != 0;
                bool doDelete = true;

                if (doRequest)
                {
                    var agentToDelete =
                        MainWindow.Instance.AgentConnnectionManager.Strategies.FirstOrDefault(
                            it => it.ActualStrategyRunning.Name == item.Name);
                    var form = new GroupAdditionDeleteMode(item.Name.ToString());
                    form.ShowDialog();
                    var selectedMode = form.SelectedDeleteMode;
                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                    {
                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                        strat.CheckPosExit();
                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                        MainWindow.Instance.DelAgentConfigBtnClick(delItem, "has been excluded from the group");
                    }
                    if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                    {
                        ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                        strat.CheckPosWaitStrExit();
                        MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                        MainWindow.Instance.DelAgentConfigBtnClick(delItem, "has been excluded from the group");
                    }
                    doDelete = !form.IsCancelled;
                }
                if (doDelete)
                {
                    DynamicGrid.Children.Remove(cb);
                    DynamicGrid.Children.Remove(tb);
                    DynamicGrid.Children.Remove(label);
                }
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
            if (!IsEditMode && WorkMode == AgentWorkMode.Single && unitItems.Count == 1)
            {
                MessageBox.Show("Group can not be composed with 1 script");
                return;
            }
            if (IsEditMode && WorkMode == AgentWorkMode.Group && unitItems.Count == 1)
            {
                MessageBox.Show("Group can not be composed with 1 script");
                return;
            }

            if (IsEditMode)
            {
                if (WorkMode == AgentWorkMode.Group)
                {
                    var oldItems = MainWindow.Instance.AgentsStorage.Where(a => a.Params.GroupName == OldGroupName).ToList();
                    List<Agent> newMembersOfCurrentGroup = new List<Agent>();

                    foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                    {

                        var x = DynamicGrid.Children.OfType<ComboBox>().ToList();
                        string cbID = cb.Name.Split('_').Last();
                        if (cbID != "")
                        {
                            foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                            {
                                var newItems = DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)).ToList();
                                var amount = ue.Text;
                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                var isInCollection = oldItems.Any(i => i.Name == algorithmName);
                                if (oldItems.Any(i => i.Name == algorithmName))
                                {
                                    var item = MainWindow.Instance.AgentsStorage.FirstOrDefault(i => i.Name == algorithmName && i.Params.GroupName == OldGroupName);
                                    item.Params.PhantomParams.AgentName = item.Name;
                                    item.Params.PhantomParams.Amount = item.Params.Amount;
                                    item.Params.PhantomParams.GroupName = item.Params.GroupName;

                                    item.Params.Amount = amount;
                                    item.Params.GroupName = groupName;
                                    var index = MainWindow.Instance.AgentsStorage.IndexOf(item);
                                    //to attache sec
                                    item.Params.Security = currentSecurities.Count > index - RowSetter ? currentSecurities[index - RowSetter] : null;
                                    MainWindow.Instance.AddNewAgentInGroup(item, index, false);
                                    //go to agent manager related actions

                                    //проверяем запущена ли группа в менеджере агентов
                                    var amItemOnTheFly = MainWindow.Instance.AgentConnnectionManager.Strategies.Where(i => i.ActualStrategyRunning.Name == item.Name && i.AgentOrGroupName == item.Params.GroupName).ToList();
                                    //проход по запущенным агентам в менеджере???
                                    foreach (var amItem in amItemOnTheFly)
                                    {
                                        if (editCount == oldItems.Count)
                                        {
                                            break;
                                        }
                                        if (amItem != null)
                                        {
                                            //если группа запущена, расширение
                                            if (DynamicGrid.Children.OfType<ComboBox>().ToList().Count < oldItems.Count)
                                            {
                                                break;
                                            }
                                            ++editCount;
                                            //go to agent manager related actions
                                            var runnigStrategy = MainWindow.Instance.AgentConnnectionManager.FirstOrDefault(i => i.ActualStrategyRunning.Name.EndsWith(item.Name));
                                            if (runnigStrategy != null)
                                            {
                                                var ueAmount = new UnitEditor();
                                                ueAmount.Text = item.Params.Amount;
                                                ueAmount.Value = ueAmount.Text.ToUnit();
                                                decimal? calculatedAmount = 0;
                                                if (ueAmount.Value.Type == UnitTypes.Percent)
                                                {
                                                    var amToCalculateAmount = MainWindow.Instance.AgentManagerStorage.FirstOrDefault(i => i.AgentManagerUniqueId == amItem.AgentOrGroupName);

                                                    calculatedAmount = MainWindow.Instance.CalculateAmount(amToCalculateAmount, item);
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
                                            //определять что сейчас идет удаление элементов группы, что присваение инструмента не требуется!
                                            //определить что выбранный агент не зарегистрирован в группе, что он реально новый член группы
                                            if (isInCollection)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                var anyActiveConnection = MainWindow.Instance.ConnectionManager.Any(i => i.ConnectionState == ConnectionStates.Connected);
                                                var cashedTools = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.ConnectionParams.Tools != null);
                                                if (!anyActiveConnection && cashedTools == null)
                                                {
                                                    MessageBox.Show("No cashed or live securities. Securities can not be selected.");
                                                    Close();
                                                    return;
                                                    //todo: добавить инфу в логи о совершенном действии
                                                }
                                            }
                                        }
                                    }
                                    newMembersOfCurrentGroup.Add(item);
                                }
                                else
                                {
                                    List<Agent> list = new List<Agent>();
                                    var agentStorageCollection = MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == "ungrouped agents").ToList();
                                    foreach (var rs in agentStorageCollection)
                                    {
                                        var newAgent = (Agent)rs.Clone();
                                        newAgent.Params.AgentCompiledName = rs.Params.AgentCompiledName;
                                        newAgent.Params.Amount = amount;
                                        newAgent.Params.GroupName = groupName;
                                        newAgent.Params.ToolTipName = rs.Params.ToolTipName;

                                        var anyActiveConnection = MainWindow.Instance.ConnectionManager.Any(i => i.ConnectionState == ConnectionStates.Connected);
                                        var cashedTools = MainWindow.Instance.ConnectionsStorage.FirstOrDefault(i => i.ConnectionParams.Tools != null);
                                        if (!anyActiveConnection && cashedTools == null)
                                        {
                                            MessageBox.Show("No cashed or live securities. Securities can not be selected.");
                                            MainWindow.Instance.AddNewAgentInGroup(newAgent, -1, false);
                                            break;
                                            //todo: добавить инфу в логи о совершенном действии
                                        }
                                        if (anyActiveConnection | cashedTools != null)
                                        {
                                            var form = new GroupAdditionSecurityPicker(newAgent);
                                            form.ShowDialog();
                                            newAgent.Params.Security = form.SelectedSecurity;
                                            MainWindow.Instance.AddNewAgentInGroup(newAgent, -1, false);
                                            form = null;
                                            //todo: добавить инфу в логи о совершенном действии
                                        }

                                        list.Add(newAgent);
                                        //проход по всем элементам коллекции менедежера агентов
                                        foreach (var item in MainWindow.Instance.AgentManagerStorage)
                                        {
                                            if (item.AgentManagerUniqueId == newAgent.Params.GroupName)
                                            {
                                                if (item.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Running)
                                                {

                                                    var runningAgents = MainWindow.Instance.AgentConnnectionManager;
                                                    var alreadyRunnig = runningAgents.Any(i => i.AgentOrGroupName == item.AgentManagerUniqueId && i.ActualStrategyRunning.Name == newAgent.Name);
                                                    if (alreadyRunnig)
                                                    {
                                                        Close();
                                                    }
                                                    else
                                                    {
                                                        MainWindow.Instance.StartAfterEdit(newAgent, item);
                                                    }
                                                }
                                                if (item.AgentManagerSettings.AgentMangerCurrentStatus == ManagerParams.AgentManagerStatus.Stopped)
                                                {
                                                    if (!anyActiveConnection && cashedTools == null)
                                                    {
                                                        MessageBox.Show("No cashed or live securities. Securities can not be selected.");
                                                        MainWindow.Instance.AddNewAgentInGroup(newAgent, -1, false);
                                                        break;
                                                        //todo: добавить инфу в логи о совершенном действии
                                                    }
                                                    var IsInGroupAlready =
                                                        MainWindow.Instance.AgentsStorage.Any(
                                                            i =>
                                                                i.Name == newAgent.Name &&
                                                                i.Params.GroupName == newAgent.Params.GroupName);
                                                    if (!IsInGroupAlready)
                                                    {
                                                        if (anyActiveConnection | cashedTools != null)
                                                        {
                                                            var form = new GroupAdditionSecurityPicker(newAgent);
                                                            form.ShowDialog();
                                                            newAgent.Params.Security = form.SelectedSecurity;
                                                            MainWindow.Instance.AddNewAgentInGroup(newAgent, -1, false);
                                                            form = null;
                                                            //todo: добавить инфу в логи о совершенном действии
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    foreach (var oldItem in oldItems)
                    {
                        var ItemsToDeleteCollection = newMembersOfCurrentGroup.Where(i => i != oldItem).ToList();
                        var IsItemToDelete = newMembersOfCurrentGroup.All(i => i != oldItem);
                        if (IsItemToDelete)
                        {
                            //если данный агент не запущен
                            var agentToDelete =
                                MainWindow.Instance.AgentConnnectionManager.Strategies.FirstOrDefault(
                                    i => i.ActualStrategyRunning.Name == oldItem.Name);
                            if (agentToDelete == null)
                            {
                                MessageBoxResult result = MessageBox.Show("Agent  will be removed from current group".Insert(6, oldItem.Name), "Delete agent from group", MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.Cancel);
                                if (result == MessageBoxResult.OK)
                                {
                                    MainWindow.Instance.DelAgentConfigBtnClick(oldItem, "has been excluded from the group");
                                    ++removeCount;
                                    if (removeCount == ItemsToDeleteCollection.Count)
                                    {
                                        removeCount = 0; //убрать после тестов
                                        break;
                                    }
                                }
                                if (result == MessageBoxResult.Cancel)
                                {
                                    ++removeCount;
                                    if (removeCount == ItemsToDeleteCollection.Count)
                                    {
                                        removeCount = 0; //убрать после тестов
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                //если данный агент запущен
                                //если у удаляемого агента есть позиции - должен быть запрос "(закрыть позиции и удалить) или (ожидать закрытия позиций и затем удалить)", с соответствующим функционалом.
                                //реализовать данный функционал

                                var form = new GroupAdditionDeleteMode(oldItem.Name.ToString());
                                form.ShowDialog();
                                var selectedMode = form.SelectedDeleteMode;
                                if (selectedMode == ManagerParams.AgentManagerDeleteMode.ClosePositionsAndDelete && !form.IsCancelled)
                                {
                                    ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                    strat.CheckPosExit();
                                    MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                    MainWindow.Instance.DelAgentConfigBtnClick(oldItem, "has been excluded from the group");
                                }
                                if (selectedMode == ManagerParams.AgentManagerDeleteMode.WaitForClosingAndDeleteAfter && !form.IsCancelled)
                                {
                                    ChStrategy strat = agentToDelete.ActualStrategyRunning as ChStrategy;
                                    strat.CheckPosWaitStrExit();
                                    MainWindow.Instance.AgentConnnectionManager.Strategies.Remove(agentToDelete);
                                    MainWindow.Instance.DelAgentConfigBtnClick(oldItem, "has been excluded from the group");
                                }
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
                        {
                            foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                            {
                                var amount = ue.Text;
                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                var items = MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == groupName).ToList();

                                int itemIndex = -1;
                                foreach (var i in items)
                                {
                                    itemIndex = MainWindow.Instance.AgentsStorage.IndexOf(i);
                                }
                                var itemToEdit = MainWindow.Instance.AgentsStorage[itemIndex];
                                itemToEdit.Params.PhantomParams.AgentName = itemToEdit.Name;
                                itemToEdit.Params.PhantomParams.Amount = itemToEdit.Params.Amount;
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
                        MainWindow.Instance.DelAgentConfigBtnClick(item, null);
                    }
                    int curr = 0;
                    foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                    {
                        string cbID = cb.Name.Split('_').Last();
                        if (cbID != "")
                        {
                            
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
                                    newAgent.Params.Security = currentSecurities[curr];
                                    newAgent.Params.ToolTipName = rs.Params.ToolTipName;
                                    list.Add(newAgent);
                                    var index = MainWindow.Instance.AgentsStorage.IndexOf(rs);
                                }
                                foreach (var i in list)
                                {
                                    MainWindow.Instance.AddNewAgentInGroup(i, -1, false);
                                }
                            }
                            ++curr;
                        }
                    }
                }
                else
                {
                    int curr = 0;
                    foreach (ComboBox cb in DynamicGrid.Children.OfType<ComboBox>())
                    {
                        string cbID = cb.Name.Split('_').Last();
                        if (cbID != "")
                        {
                            foreach (UnitEditor ue in DynamicGrid.Children.OfType<UnitEditor>().Where(c => c.Name.EndsWith(cbID)))
                            {
                                var amount = ue.Text;
                                string algorithmName = cb.Text;
                                var groupName = GroupNameTxtBox.Text;
                                var items = MainWindow.Instance.AgentsStorage.Where(a => a.Params.FriendlyName == algorithmName && a.Params.GroupName == groupName).ToList();
                                int itemIndex = -1;
                                foreach (var i in items)
                                {
                                    itemIndex = MainWindow.Instance.AgentsStorage.IndexOf(i);
                                }
                                var itemToEdit = MainWindow.Instance.AgentsStorage[itemIndex];
                                itemToEdit.Params.Amount = amount;
                                itemToEdit.Params.GroupName = groupName;
                                itemToEdit.Name = algorithmName;
                                itemToEdit.Params.Security = currentSecurities[curr];
                                MainWindow.Instance.AddNewAgent(itemToEdit, EditIndex);
                            }
                        }
                    }
                }
                Close();
            }
        }

        private void StratSettings_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsEditMode)
            {
                AddConfigBtnClick(sender, e);
            }
            CreateGroupBtnHelper();
        }

        private void CreateGroupBtnHelper()
        {
            CreateGroupeBtn.IsEnabled = DynamicGrid.Children.Count != 0;
        }
    }
}
