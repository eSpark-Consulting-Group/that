using APTAT.Core.CMC.UFH;

namespace APTAT.Core.CMC.Request
{
    public class ValkyrieUnkownFileUpload : IValkyrieUnkownFileUpload
    {
        public int TotalFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
    }

    public class ValkyrieUnkownFileUploadWithTriggerRequest : IValkyrieUnkownFileUploadWithTrigger
    {
        public int TotalFilesCount { get; set; }
        public int MalwareFilesCount { get; set; }
        public ValkyrieUnkownFileUploadTrigger UploadTrigger { get; set; }
    }
}
