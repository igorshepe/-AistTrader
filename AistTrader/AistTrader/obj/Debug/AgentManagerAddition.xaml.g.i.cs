﻿#pragma checksum "..\..\AgentManagerAddition.xaml.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "9E63235E77B656469F11A2743660C032"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using StockSharp.Xaml;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace AistTrader {
    
    
    /// <summary>
    /// AgentManagerAddition
    /// </summary>
    public partial class AgentManagerAddition : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 4 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AistTrader.AgentManagerAddition StratSettings;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label AmountLbl;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label UnitVolumeLabel;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox AccountComboBox;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox GroupOrSingleAgentComboBox;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox ToolComboBox;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\AgentManagerAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal StockSharp.Xaml.UnitEditor AmountTextBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/agentmanageraddition.xaml.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AgentManagerAddition.xaml.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.StratSettings = ((AistTrader.AgentManagerAddition)(target));
            return;
            case 2:
            this.AmountLbl = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.UnitVolumeLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.AccountComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 51 "..\..\AgentManagerAddition.xaml.xaml"
            this.AccountComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AccountComboBox_OnSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.GroupOrSingleAgentComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 57 "..\..\AgentManagerAddition.xaml.xaml"
            this.GroupOrSingleAgentComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.GroupOrSingleAgentComboBox_OnSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ToolComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 7:
            this.AmountTextBox = ((StockSharp.Xaml.UnitEditor)(target));
            
            #line 78 "..\..\AgentManagerAddition.xaml.xaml"
            this.AmountTextBox.KeyUp += new System.Windows.Input.KeyEventHandler(this.AmountTextBox_KeyUp);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 84 "..\..\AgentManagerAddition.xaml.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AddConfigBtnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

