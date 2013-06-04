using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core
{
    public class ScanResult
    {
        public Guid TargetUid { get; set; }
        public string FileName { get; set; }
        public int TotalFilesCount { get; set; }
        public int UnknownFilesCount { get; set; }

        public List<UnknownFile> UnknownFiles { get; set; }
    }

    public class UnknownFile
    {
        public string Name { get; set; }
        public long Size { get; set; }
    }
}
