namespace APTAT.Core.Valkyrie
{
    public class ValkyrieAnalyseOperationContext
    {
        public IValkyrieFileInfo ValkyrieFileInfo { get; set; }
        
        public string AnalysisId { get; set; }
        public string HashSha1 { get; set; }
    }
}
