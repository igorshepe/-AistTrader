﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Common.Settings;

namespace AistTrader.Properties {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("")]
        public string Setting {
            get {
                return ((string)(this["Setting"]));
            }
        }
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        public SettingsArrayList Robots
        {
            get
            {
                return ((SettingsArrayList)(this["Robots"]));
            }
            set
            {
                this["Robots"] = value;
            }
        }

        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("")]
        public string WorkingPath
        {
            get
            {
                return ((string)(this["WorkingPath"]));
            }
            set
            {
                this["WorkingPath"] = value;
            }
        }

        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("False")]
        public bool DropBoxEnabled
        {
            get
            {
                return ((bool)(this["DropBoxEnabled"]));
            }
            set
            {
                this["DropBoxEnabled"] = value;
            }
        }

        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        public SettingsArrayList Agents
        {
            get
            {
                return ((SettingsArrayList)(this["Agents"]));
            }
            set
            {
                this["Agents"] = value;
            }
        }
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        public SettingsArrayList AgentConnection
        {
            get
            {
                return ((SettingsArrayList)(this["AgentConnection"]));
            }
            set
            {
                this["AgentConnection"] = value;
            }
        }
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        public SettingsArrayList AgentPortfolio
        {
            get
            {
                return ((SettingsArrayList)(this["AgentPortfolio"]));
            }
            set
            {
                this["AgentPortfolio"] = value;
            }
        }
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        public SettingsArrayList AgentManager
        {
            get
            {
                return ((SettingsArrayList)(this["AgentManager"]));
            }
            set
            {
                this["AgentManager"] = value;
            }
        }

    }
}
