using System;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Valkyrie
{
    public interface IValkyrieAnalysisResult
    {
        DateTime AnalysisDate { get; }
        string AnalyzeId { get; }
        DateTime? FirstAnalysisDate { get; }
        bool IsSkipped { get; }
        bool IsSubmitted { get; }
        bool IsWhiteListed { get; }
        DateTime? LastAnalysisDate { get; }
        float? MalwareProbability { get; }
        string OperationStatus { get; }
        ErrorCodesEnum ReturnCode { get; }
        string Sha1 { get; }
        string SkipReason { get; }
        SkippReasonEnum SkipReasonEnum { get; }
        ValkyrieVerdict Verdict { get; }
    }
}