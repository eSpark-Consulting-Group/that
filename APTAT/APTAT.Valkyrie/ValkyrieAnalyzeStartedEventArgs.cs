using System;

namespace APTAT.Valkyrie
{
    public class ValkyrieAnalyzeStartedEventArgs : EventArgs
    {
        public ValkyrieAnalyzeStartedEventArgs(string apiKey, string scanId)
        {
            ApiKey = apiKey;
            ScanId = scanId;
        }
        public string ApiKey { get; }
        public string ScanId { get; }
    }
}