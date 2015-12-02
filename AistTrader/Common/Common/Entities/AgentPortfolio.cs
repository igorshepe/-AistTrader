using System;

namespace Common.Entities
{
    [Serializable]
    public class AgentPortfolio
    {
        AgentPortfolio() { }
        public AgentPortfolio(string name, AgentConnection connection)
        {
            Name = name;
            Connection = connection;
        }
        public string Name { get; set; }
        public AgentConnection Connection { get; set; }
    }
}
