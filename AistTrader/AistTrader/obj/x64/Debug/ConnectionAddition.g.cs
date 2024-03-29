﻿#pragma checksum "..\..\..\ConnectionAddition.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "46A4354BD569087B25661C0C610301AD"
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
    /// ConnectionAddition
    /// </summary>
    public partial class ConnectionAddition : MahApps.Metro.Controls.MetroWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 15 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AistTrader.ConnectionAddition Window;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid AgentConnectionGrid;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label PathToPlazaLable;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.TextBox ConnectionNameTxtBox;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.TextBox ClienCodeTxtBox;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OkAgentConnectionBtn;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox AllPlazaDirectoriesComboBox;
        
        #line default
        #line hidden
        
        
        #line 115 "..\..\..\ConnectionAddition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox IsDemoChkBox;
        
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
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/connectionaddition.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\ConnectionAddition.xaml"
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
            this.Window = ((AistTrader.ConnectionAddition)(target));
            return;
            case 2:
            this.AgentConnectionGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.PathToPlazaLable = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.ConnectionNameTxtBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 71 "..\..\..\ConnectionAddition.xaml"
            this.ConnectionNameTxtBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.ConnectionNameTxtBox_KeyDown);
            
            #line default
            #line hidden
            
            #line 72 "..\..\..\ConnectionAddition.xaml"
            this.ConnectionNameTxtBox.KeyUp += new System.Windows.Input.KeyEventHandler(this.ConnectionNameTxtBox_OnKeyUp);
            
            #line default
            #line hidden
            
            #line 73 "..\..\..\ConnectionAddition.xaml"
            this.ConnectionNameTxtBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.ConnectionNameTxtBox_OnTextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ClienCodeTxtBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 81 "..\..\..\ConnectionAddition.xaml"
            this.ClienCodeTxtBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.ConnectionNameTxtBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.OkAgentConnectionBtn = ((System.Windows.Controls.Button)(target));
            
            #line 98 "..\..\..\ConnectionAddition.xaml"
            this.OkAgentConnectionBtn.Click += new System.Windows.RoutedEventHandler(this.OkBtnClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.AllPlazaDirectoriesComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 8:
            this.IsDemoChkBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

