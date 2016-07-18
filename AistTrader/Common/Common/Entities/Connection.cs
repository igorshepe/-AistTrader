using System;
using System.Runtime.Serialization;
using Common.Params;

namespace Common.Entities
{
    [DataContract(Namespace = "")]
    public class Connection
    {
        Connection() { }
        public Connection(string name, ConnectionParams connectionParams,bool isDemo)
        {
            Id = name;
            DisplayName = name;
            ConnectionParams = connectionParams;
            IsDemo = isDemo;
        }
        [DataMember()]
        public string Id { get; set; }
        [DataMember()]
        public string DisplayName { get; set; }
        [DataMember()]
        public bool IsDemo { get; set; }
        [DataMember()]
        public ConnectionParams ConnectionParams { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
