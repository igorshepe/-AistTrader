using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Entities;

namespace Common.Settings
{
    [XmlInclude(typeof(Agent))]
    [XmlInclude(typeof(AgentConnection))]
    [XmlInclude(typeof(AgentPortfolio))]
    [XmlInclude(typeof(AgentManager))]
    public class SettingsArrayList : ArrayList
    {
        // ReSharper disable UnusedMember.Global
        public SettingsArrayList() { }
        // ReSharper restore UnusedMember.Global
        public SettingsArrayList(ICollection c) : base(c) { }
    }
}
