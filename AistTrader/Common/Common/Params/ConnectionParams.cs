﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using StockSharp.BusinessEntities;

namespace Common.Params
{
    [DataContract(Namespace = "")]
    public class ConnectionParams
    {
        public ConnectionParams(string name, string code, PlazaConnectionParams connectiong, bool isActive,bool isDefauld)
        {
            Name = name;
            Code = code;
            PlazaConnectionParams = connectiong;
            IsConnected = isActive;
            Funds = -1;
            AlgorithmCount = -1;
            Contracts = -1;
            NetValue = -1;
            VariationMargin = -1;
            ConnectionState = ConnectionStatus.Disconnected;
            IsDefaulConnection = isDefauld;
        }   

        
        public ConnectionParams(){}
        public override string ToString() { return Name; }
        public bool IsDefaulConnection { get; set; }
        public PlazaConnectionParams PlazaConnectionParams { get; set; }
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
        [XmlIgnore]
        public List<Security> Tools { get; set; }
        [XmlIgnore]
        public List<Portfolio> Accounts { get; set; }
        [XmlIgnore]
        public Portfolio SelectedAccount { get; set; }
        public ConnectionStatus ConnectionState { get; set; }

        public enum ConnectionStatus
        {
            Connected,
            Disconnected,
            ConnectionError,
            Authentication
        }
    }

    [TypeConverter]
    public enum OperationCommand
    {
        Connect,
        Disconnect
    };
    
    public enum ConnectionType
    {
        [Description("Plaza")]
        Plaza
    };
    
    [DataContract(Namespace = "")]
    public class ConnectionsPhantomParams
    {
        public ConnectionsPhantomParams() { }
        //public ConnectionsPhantomParams()
        //{
           
        //}
    
    }
}
