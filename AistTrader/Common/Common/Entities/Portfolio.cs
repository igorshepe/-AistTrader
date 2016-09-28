using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Common.Entities
{
    [DataContract(Namespace = "")]
    public class Portfolio
    {
        Portfolio() { }

        public Portfolio(string name, Connection connection, string code, StockSharp.BusinessEntities.Portfolio portfolio)
        {
            Id = Guid.NewGuid();
            Name = name;
            Connection = connection;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }

        [DataMember()]
        public Guid Id { get; set; }
        [DataMember()]
        public string Name { get; set; }
        [DataMember()]
        public Connection Connection { get; set; }
        [DataMember()]
        public string Code { get; set; }
    }
}
