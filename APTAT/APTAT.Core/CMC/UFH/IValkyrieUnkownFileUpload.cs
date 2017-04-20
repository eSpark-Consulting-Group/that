namespace APTAT.Core.CMC.UFH
{
    public enum ValkyrieUnkownFileUploadTrigger
    {
        AfterScan, AnalyzeButton
    }

    public interface IValkyrieUnkownFileUpload : IRequest
    {
        int TotalFilesCount { get; set; }
        int MalwareFilesCount { get; set; }
        
    }
    public interface IValkyrieUnkownFileUploadWithTrigger : IValkyrieUnkownFileUpload
    {
        ValkyrieUnkownFileUploadTrigger UploadTrigger { get; set; }

    }
}