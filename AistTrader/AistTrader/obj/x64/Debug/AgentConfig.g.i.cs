﻿#pragma checksum "..\..\..\AgentConfig.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B9CA2FE2CE74D092AD66906065166B14"
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
    /// AgentConfig
    /// </summary>
    public partial class AgentConfig : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 4 "..\..\..\AgentConfig.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AistTrader.AgentConfig AddAgentConfig;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\AgentConfig.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox AlgorithmComboBox;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\AgentConfig.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.Button AgentSettingsButton;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\AgentConfig.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AlgorithmOkBtn;
        
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
            System.Uri resourceLocater = new System.Uri("/AistTrader;component/agentconfig.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AgentConfig.xaml"
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
            this.AddAgentConfig = ((AistTrader.AgentConfig)(target));
            return;
            case 2:
            this.AlgorithmComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 37 "..\..\..\AgentConfig.xaml"
            this.AlgorithmComboBox.Loaded += new System.Windows.RoutedEventHandler(this.AlgorithmComboBox_Loaded);
            
            #line default
            #line hidden
            
            #line 38 "..\..\..\AgentConfig.xaml"
            this.AlgorithmComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AlgorithmComboBoxSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.AgentSettingsButton = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\..\AgentConfig.xaml"
            this.AgentSettingsButton.Click += new System.Windows.RoutedEventHandler(this.AgentSettingsButtonClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.AlgorithmOkBtn = ((System.Windows.Controls.Button)(target));
            
            #line 56 "..\..\..\AgentConfig.xaml"
            this.AlgorithmOkBtn.Click += new System.Windows.RoutedEventHandler(this.AddConfigBtnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

