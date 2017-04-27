using System;

namespace APTAT.Core.Reports
{
    public class ReportFile
    {        
        public string FileName { get; set; }
        public Exception Error { get; set; }
        public byte[] DocumentBytes { get; set; }
        public Boolean IsGenerated { get; set; }
    }
}
