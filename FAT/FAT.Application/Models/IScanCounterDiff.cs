namespace FAT.Application.Models
{
    public interface IScanCounterDiff
    {
        int CleanFilesDiff { get; set; }
        int InProgressFilesDiff { get; set; }
        int MaliciousFilesDiff { get; set; }
        int NotAnalyzedFilesDiff { get; set; }
        int UnknownFilesDiff { get; set; }
    }
}