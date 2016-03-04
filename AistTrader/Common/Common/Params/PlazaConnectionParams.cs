using System;

namespace Common.Params
{
    [Serializable]
    public class PlazaConnectionParams
    {
        PlazaConnectionParams() { }
        public PlazaConnectionParams( string path)
        {
            Path = path;
        }
        public string Path { get; set; }
        public string IpEndPoint { get; set; }
    }
}
