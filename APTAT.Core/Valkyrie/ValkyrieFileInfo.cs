using System;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieFileInfo
    {
        public string Path { get; set; }
        public string Sha1 { get; set; }
        public string Ip { get; set; }

        public string GetFullPath()
        {
            return String.Format("\\\\{0}\\{1}", Ip, Path);
        }
    }
}
