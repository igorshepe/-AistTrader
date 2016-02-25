using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using StockSharp.BusinessEntities;
using StockSharp.Plaza;

namespace Common.Settings
{
    [Serializable]
    public class ConnectionsSettings
    {
        public ConnectionsSettings(string name, string code, TerminalSettings connectiong, bool isActive)
        {
            Name = name;
            Code = code;
            ConnectionSettings = connectiong;
            IsConnected = isActive;
            Funds = -1;
            AlgorithmCount = -1;
            Contracts = -1;
            NetValue = -1;
            VariationMargin = -1;
            ConnectionStatus = AgentConnectionStatus.Disconnected;

        }

        public ConnectionsSettings()
        {
        }
        public TerminalSettings ConnectionSettings { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsConnected { get; set; }
        public OperationCommand Command { get; set; }
        //TODO: уточнить типы данных
        public decimal Funds { get; set; }
        public int AlgorithmCount { get; set; }
        public int Contracts { get; set; }
        public decimal NetValue { get; set; }
        public decimal VariationMargin { get; set; }
        public bool IsRegistredConnection { get; set; }

        //TODO: подключить с новыми исходниками
        public List<Security> Tools { get; set; }
        public List<Portfolio> Accounts { get; set; }
        public Portfolio SelectedAccount { get; set; }
        public AgentConnectionStatus ConnectionStatus { get; set; }


        public enum AgentConnectionStatus
        {
            Connected,
            Disconnected,
            ConnectionError,
            Authentication
            //TODO:дополнить по необходимости тем, что надо
        }
        
    }


    [TypeConverter]
    public enum OperationCommand
    {
        Connect,
        Disconnect
    };
    public enum TerminalType
    {
        [Description("Plaza")]
        Plaza
    };
}
