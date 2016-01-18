﻿#pragma checksum "..\..\AgentConnectionAddition.xaml.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B896B8763A046E9209ABEF6071DBA048"
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
    /// AgentConnectionAddition
    /// </summary>
    public partial class AgentConnectionAddition : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 37 "..\..\AgentConnectionAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label PathToPlazaLable;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\AgentConnectionAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.TextBox ClienNameTxtBox;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\AgentConnectionAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.TextBox ClienCodeTxtBox;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\AgentConnectionAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox ConnectionTypeComboBox;
        
        #line default
        #line hidden
        
        
        #line 112 "..\..\AgentConnectionAddition.xaml.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.ComboBox AllPlazaDirectoriesComboBox;
        
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
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/agentconnectionaddition.xaml.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AgentConnectionAddition.xaml.xaml"
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
            this.PathToPlazaLable = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.ClienNameTxtBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 53 "..\..\AgentConnectionAddition.xaml.xaml"
            this.ClienNameTxtBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.ClienNameTxtBox_KeyDown);
            
            #line default
            #line hidden
            
            #line 54 "..\..\AgentConnectionAddition.xaml.xaml"
            this.ClienNameTxtBox.KeyUp += new System.Windows.Input.KeyEventHandler(this.ClienNameTxtBox_OnKeyUp);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ClienCodeTxtBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 64 "..\..\AgentConnectionAddition.xaml.xaml"
            this.ClienCodeTxtBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.ClienNameTxtBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ConnectionTypeComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 74 "..\..\AgentConnectionAddition.xaml.xaml"
            this.ConnectionTypeComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.TerminalComboBoxSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 99 "..\..\AgentConnectionAddition.xaml.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OkBtnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.AllPlazaDirectoriesComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 116 "..\..\AgentConnectionAddition.xaml.xaml"
            this.AllPlazaDirectoriesComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AllPlazaDirectoriesComboBoxSelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
