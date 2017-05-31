namespace FAT.Application.Models
{
    public interface IScanCounterDiff
    {
        int CleanDiff { get; set; }
        int MalwareDiff { get; set; }
        int UnknownDiff { get; set; }
        int NotAnalyzedDiff { get; set; }
        int InProgressDiff { get; set; }
        int NewUnknownFilesCount { get; set; }
    }

    public class ScanCounterDiff : IScanCounterDiff
    {
        public int CleanDiff { get; set; }
        public int MalwareDiff { get; set; }
        public int UnknownDiff { get; set; }
        public int NotAnalyzedDiff { get; set; }
        public int InProgressDiff { get; set; }
        public int NewUnknownFilesCount { get; set; }
    }
}