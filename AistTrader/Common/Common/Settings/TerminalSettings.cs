using System;

namespace Common.Settings
{
    [Serializable]
    public class TerminalSettings
    {
        TerminalSettings() { }
        public TerminalSettings(TerminalType type, string path)
        {
            Type = type;
            Path = path;
        }

        public TerminalType Type { get; set; }
        public string Path { get; set; }
        public string IpEndPoint { get; set; }
    }
}
