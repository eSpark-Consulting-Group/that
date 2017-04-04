using System;
namespace APTAT.Core.Reports
{
    public class VulnerableDeviceInfoEx
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }
        public int BadFilesCount { get; set; }
        public string BadFileType { get; set; }
    }
}
