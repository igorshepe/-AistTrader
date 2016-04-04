﻿using System;
using Common.Params;

namespace Common.Entities
{
    //ToDO: переименовать соотвественно
    [Serializable]
    public class Connection
    {
        Connection() { }
        public Connection(string name, ConnectionParams connectionParams)
        {
            Id = name;
            DisplayName = name;
            ConnectionParams = connectionParams;
        }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ConnectionParams ConnectionParams { get; set; }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
