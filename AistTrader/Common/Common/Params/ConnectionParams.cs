using System;
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
        [DataMember()]
        public bool IsDefaulConnection { get; set; }
        [DataMember()]
        public PlazaConnectionParams PlazaConnectionParams { get; set; }
        [DataMember()]
        public string Name { get; set; }
        [DataMember()]
        public string Code { get; set; }
        [DataMember()]
        public string Login { get; set; }
        [DataMember()]
        public string Password { get; set; }
        [DataMember()]
        public bool IsConnected { get; set; }
        [DataMember()]
        public OperationCommand Command { get; set; }
        //TODO: уточнить типы данных
        [DataMember()]
        public decimal Funds { get; set; }
        [DataMember()]
        public int AlgorithmCount { get; set; }
        [DataMember()]
        public int Contracts { get; set; }
        [DataMember()]
        public decimal NetValue { get; set; }
        [DataMember()]
        public decimal VariationMargin { get; set; }
        [DataMember()]
        public bool IsRegistredConnection { get; set; }

        [DataMember()]
        public List<Security> Tools { get; set; }
        [DataMember()]
        public List<Portfolio> Accounts { get; set; }
        [DataMember()]
        public Portfolio SelectedAccount { get; set; }
        [DataMember()]
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
