﻿#pragma checksum "..\..\..\AgentPortfolioAddition.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5A750D25BDFA8840ECEE4B069E6FFB3F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
    /// AgentPortfolioAddition
    /// </summary>
    public partial class AgentPortfolioAddition : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\AgentPortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AistTrader.AgentPortfolioAddition Window;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\AgentPortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox ConnectionProviderComboBox;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\AgentPortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox AccountComboBox;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\AgentPortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.TextBox PortfolioNameTxtBox;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\AgentPortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OkPortfolioBtn;
        
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
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/agentportfolioaddition.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AgentPortfolioAddition.xaml"
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
            this.Window = ((AistTrader.AgentPortfolioAddition)(target));
            return;
            case 2:
            this.ConnectionProviderComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 54 "..\..\..\AgentPortfolioAddition.xaml"
            this.ConnectionProviderComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ConnectionProviderComboBox_OnSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.AccountComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 63 "..\..\..\AgentPortfolioAddition.xaml"
            this.AccountComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AccountComboBoxSelectionChanged);
            
            #line default
            #line hidden
            
            #line 64 "..\..\..\AgentPortfolioAddition.xaml"
            this.AccountComboBox.Loaded += new System.Windows.RoutedEventHandler(this.AccountComboBox_OnLoaded);
            
            #line default
            #line hidden
            return;
            case 4:
            this.PortfolioNameTxtBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.OkPortfolioBtn = ((System.Windows.Controls.Button)(target));
            
            #line 78 "..\..\..\AgentPortfolioAddition.xaml"
            this.OkPortfolioBtn.Click += new System.Windows.RoutedEventHandler(this.OkBtnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

