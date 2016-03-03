using System;
using Common.Settings;

namespace Common.Entities
{
    //ToDO: переименовать соотвественно
    [Serializable]
    public class AgentConnection
    {
        AgentConnection() { }
        public AgentConnection(string name, ConnectionsSettings connectionSettings)
        {
            Name = name;
            Connection = connectionSettings;
        }
        public string Name { get; set; }
        public ConnectionsSettings Connection { get; set; }

    }
}
