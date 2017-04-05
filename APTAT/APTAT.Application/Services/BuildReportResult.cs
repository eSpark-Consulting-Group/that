using System;

namespace APTAT.Application.Services
{
    public class BuildReportResult
    {
        public String FileName { get; private set; }
        public byte[] DocumentBytes { get; private set; }
        public Exception Exception { get; private set; }
        public BuildReportResult(string fileName, byte[] documentBytes, Exception e)
        {
            DocumentBytes = documentBytes;
            Exception = e;
            FileName = fileName;
        }
    }
}
