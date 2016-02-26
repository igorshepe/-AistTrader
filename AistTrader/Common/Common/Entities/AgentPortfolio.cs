using System;

namespace Common.Entities
{
    [Serializable]
    public class AgentPortfolio
    {
        AgentPortfolio() { }
        public AgentPortfolio(string name, AgentConnection connection,string code)
        {
            Name = name;
            Connection = connection;
            Code = code;
        }
        public string Name { get; set; }
        public AgentConnection Connection { get; set; }
        public string Code { get; set; }
    }
}
