﻿#pragma checksum "..\..\..\PortfolioAddition.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "959A8731DC774AFB7B79B318E5D84583"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro.Controls;
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
    /// PortfolioAddition
    /// </summary>
    public partial class PortfolioAddition : MahApps.Metro.Controls.MetroWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\PortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AistTrader.PortfolioAddition Window;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\PortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox ConnectionProviderComboBox;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\PortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox AccountComboBox;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\PortfolioAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.TextBox PortfolioNameTxtBox;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\..\PortfolioAddition.xaml"
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
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/portfolioaddition.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\PortfolioAddition.xaml"
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
            this.Window = ((AistTrader.PortfolioAddition)(target));
            return;
            case 2:
            this.ConnectionProviderComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 58 "..\..\..\PortfolioAddition.xaml"
            this.ConnectionProviderComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.Selector_OnSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.AccountComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 68 "..\..\..\PortfolioAddition.xaml"
            this.AccountComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AccountComboBoxSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.PortfolioNameTxtBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.OkPortfolioBtn = ((System.Windows.Controls.Button)(target));
            
            #line 82 "..\..\..\PortfolioAddition.xaml"
            this.OkPortfolioBtn.Click += new System.Windows.RoutedEventHandler(this.OkBtnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
