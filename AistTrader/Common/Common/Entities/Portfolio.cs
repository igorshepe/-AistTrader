using System;
using System.Xml.Serialization;

namespace Common.Entities
{
    [Serializable]
    [XmlType(Namespace = "Common.Entities", TypeName = "Common.Entities.Portfolio")]
    public class Portfolio
    {
        Portfolio() { }
        public Portfolio(string name, Connection connection,string code,StockSharp.BusinessEntities.Portfolio portfolio)
        {
            Name = name;
            Connection = connection;
            Code = code;
            SelectedAccount = portfolio;
        }
        public override string ToString()
        {
            return Name;
        }
        public string Name { get; set; }
        public Connection Connection { get; set; }
        public string Code { get; set; }
        [XmlIgnore]
        public StockSharp.BusinessEntities.Portfolio SelectedAccount { get; set; }
    }
}
