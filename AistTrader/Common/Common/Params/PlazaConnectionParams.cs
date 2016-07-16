using System;
using System.Runtime.Serialization;

namespace Common.Params
{
    [DataContract(Namespace = "")]
    public class PlazaConnectionParams
    {
        PlazaConnectionParams() { }
        public PlazaConnectionParams( string path)
        {
            Path = path;
        }
        [DataMember()]
        public string Path { get; set; }
        [DataMember()]
        public string IpEndPoint { get; set; }
    }
}
