using System;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;

namespace ValkyrieHashAnalyzer
{
    public class ValkyrieAnalysisResult
    {
        public static ValkyrieAnalysisResult Create(BasicInfoResponse basicInfo)
        {
            ValkyrieAnalysisResult result = new ValkyrieAnalysisResult();
            result.SHA1 = basicInfo.SHA1;
            result.FirstAnalysisDate = basicInfo.FirstAnalysisDate;
            result.LastAnalysisDate = basicInfo.LastAnalysisDate;
            result.MalwareProbability = basicInfo.MalwareProbability;
            result.Verdict = basicInfo.Verdict;
            result.IsWhiteListed = basicInfo.IsWhiteListed;
            result.ReturnCode = basicInfo.ReturnCode;

            result.OperationStatus = result.ReturnCode.ToString();

            return result;
        }

        public static ValkyrieAnalysisResult Create(CheckAnalysisResultResponse automaticResponse)
        {
            ValkyrieAnalysisResult result = new ValkyrieAnalysisResult();
            result.SHA1 = automaticResponse.SHA1;
            result.FirstAnalysisDate = automaticResponse.FirstAnalysisDate;
            result.LastAnalysisDate = automaticResponse.LastAnalysisDate;
            result.Verdict = automaticResponse.Verdict;
            result.IsWhiteListed = automaticResponse.IsWhiteListed;
            result.MalwareProbability = automaticResponse.MalwareProbability;
            result.ReturnCode = automaticResponse.ReturnCode;
            result.OperationStatus = result.ReturnCode.ToString();
            return result;
        }

        public string SHA1 { get; private set; }
        public string FilePath { get; private set; }
        public bool IsWhiteListed { get; private set; }
        public DateTime? LastAnalysisDate { get; private set; }
        public InfoVerdictEnum Verdict { get; private set; }

        public string VerdictString
        {

            get
            {
                if (Verdict == InfoVerdictEnum.NotAnalyzed) return "Not Analyzed";
                if (Verdict == InfoVerdictEnum.PotentiallyUnwantedApplication) return "Potentially Unwanted";

                if (Verdict == InfoVerdictEnum.Undetected) return "No Threat Found";

                return Verdict.ToString();
            }
        }

        public bool IsSkipped { get; private set; }
        public string SkipReason { get; private set; }

        public static ValkyrieAnalysisResult CreateSkipInfo(string sha1, string reason, SkippReasonEnum skippReasonEnum)
        {
            var result = new ValkyrieAnalysisResult() { IsSkipped = true, SHA1 = sha1, SkipReason = reason, Verdict = InfoVerdictEnum.NotAnalyzed, IsWhiteListed = false, LastAnalysisDate = null };
            switch (skippReasonEnum)
            {
                case SkippReasonEnum.Skipped:
                    result.OperationStatus = "Exceed size limit (25 MB).";
                    break;
                case SkippReasonEnum.Cancelled:
                    result.OperationStatus = "Cancelled by user.";
                    break;
                case SkippReasonEnum.Failed:
                    result.OperationStatus = reason;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("skippReasonEnum", skippReasonEnum, null);
            }
            return result;
        }

        public float MalwareProbability { get; set; }
        public DateTime? FirstAnalysisDate { get; set; }
        public ErrorCodesEnum ReturnCode { get; set; }
        public string OperationStatus { get; set; }
    }
}
