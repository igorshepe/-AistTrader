﻿#pragma checksum "..\..\..\AgentManagerAddition.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C76FD12D9BCE83E794219DE3F2EAD862"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using StockSharp.Licensing.Xaml;
using StockSharp.Xaml;
using StockSharp.Xaml.Charting;
using StockSharp.Xaml.PropertyGrid;
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
        
        
        #line 4 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AistTrader.AgentManagerAddition StratSettings;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label AmountLbl;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label UnitVolumeLabel;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox AccountComboBox;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox GroupOrSingleAgentComboBox;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal StockSharp.Xaml.SecurityEditor SecurityPicker;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal StockSharp.Xaml.UnitEditor AmountTextBox;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\..\AgentManagerAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OkBtnClick;
        
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
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/agentmanageraddition.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AgentManagerAddition.xaml"
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
            
            #line 73 "..\..\..\AgentManagerAddition.xaml"
            this.AccountComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AccountComboBox_OnSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.GroupOrSingleAgentComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 79 "..\..\..\AgentManagerAddition.xaml"
            this.GroupOrSingleAgentComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.GroupOrSingleAgentComboBox_OnSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.SecurityPicker = ((StockSharp.Xaml.SecurityEditor)(target));
            return;
            case 7:
            this.AmountTextBox = ((StockSharp.Xaml.UnitEditor)(target));
            
            #line 106 "..\..\..\AgentManagerAddition.xaml"
            this.AmountTextBox.KeyUp += new System.Windows.Input.KeyEventHandler(this.AmountTextBox_KeyUp);
            
            #line default
            #line hidden
            return;
            case 8:
            this.OkBtnClick = ((System.Windows.Controls.Button)(target));
            
            #line 114 "..\..\..\AgentManagerAddition.xaml"
            this.OkBtnClick.Click += new System.Windows.RoutedEventHandler(this.AddConfigBtnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

