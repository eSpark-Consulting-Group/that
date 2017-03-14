namespace APTAT.Core
{
    public interface IFlsResponse
    {
        IFlsResult[] GetAnalyzed();
        string[] GetFailedToAnalyzeHashes();
    }
}