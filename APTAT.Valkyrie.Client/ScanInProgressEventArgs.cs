using System;

namespace APTAT.Valkyrie.Client
{
    public class InProgressEventArgs: EventArgs
    {
        public int Percentage { get; set; }
        public int TotalFileCount { get; set; }
        public int ProcessedFileCount { get; set; }
    }
}