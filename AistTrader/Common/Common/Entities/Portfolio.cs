using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Common.Entities
{
    [DataContract(Namespace = "")]
    public class Portfolio
    {
        Portfolio() { }
        public Portfolio(string name, Connection connection,string code, StockSharp.BusinessEntities.Portfolio portfolio)
        {
            Name = name;
            Connection = connection;
            Code = code;
            //SelectedAccount = portfolio;
        }
        public override string ToString()
        {
            return Name;
        }
        [DataMember()]
        public string Name { get; set; }
        [DataMember()]
        public Connection Connection { get; set; }
        [DataMember()]
        public string Code { get; set; }
        
        //public StockSharp.BusinessEntities.Portfolio SelectedAccount { get; set; }
    }
}
