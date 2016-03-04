using System;
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
            Name = name;
            ConnectionParams = connectionParams;
        }
        public string Name { get; set; }
        public ConnectionParams ConnectionParams { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
