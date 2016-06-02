using System;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieAnalysisResult
    {
        public static ValkyrieAnalysisResult Create(BasicInfoResponse basicInfo)
        {
            ValkyrieAnalysisResult result = new ValkyrieAnalysisResult
            {
                SHA1 = basicInfo.SHA1,
                FirstAnalysisDate = basicInfo.FirstAnalysisDate,
                LastAnalysisDate = basicInfo.LastAnalysisDate,
                MalwareProbability = basicInfo.MalwareProbability,
                Verdict = basicInfo.Verdict,
                IsWhiteListed = basicInfo.IsWhiteListed,
                ReturnCode = basicInfo.ReturnCode
            };


            result.OperationStatus = result.ReturnCode.ToString();
          
            return result;
        }

        

        public static ValkyrieAnalysisResult Create(CheckAnalysisResultResponse automaticResponse)
        {
            ValkyrieAnalysisResult result = new ValkyrieAnalysisResult
            {
                SHA1 = automaticResponse.SHA1,
                FirstAnalysisDate = automaticResponse.FirstAnalysisDate,
                LastAnalysisDate = automaticResponse.LastAnalysisDate,
                Verdict = automaticResponse.Verdict,
                IsWhiteListed = automaticResponse.IsWhiteListed,
                MalwareProbability = automaticResponse.MalwareProbability,
                ReturnCode = automaticResponse.ReturnCode
            };
            result.OperationStatus = result.ReturnCode.ToString();
            return result;
        }

        public string SHA1 { get; private set; }
        public string FilePath { get; private set; }
        public bool IsWhiteListed { get; private set; }
        public DateTime? LastAnalysisDate { get; private set; }
        public ValkyrieVerdict Verdict { get; private set; }

        public string VerdictString
        {

            get
            {
                if (Verdict == ValkyrieVerdict.NotAnalyzed) return "Not Analysed";
                if (Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication) return "Potentially Unwanted";

                if (Verdict == ValkyrieVerdict.Undetected) return "No Threat Found";

                return Verdict.ToString();
            }
        }

        public bool IsSkipped { get; private set; }
        public string SkipReason { get; private set; }

        public static ValkyrieAnalysisResult CreateSkipInfo(string sha1, string reason, SkippReasonEnum skippReasonEnum)
        {
            var result = new ValkyrieAnalysisResult() {IsSkipped = true, SHA1 = sha1, SkipReason = reason, Verdict = ValkyrieVerdict.NotAnalyzed , IsWhiteListed = false, LastAnalysisDate = null};
            switch (skippReasonEnum)
            {
                case SkippReasonEnum.Skipped:
                    result.OperationStatus = "Exceed size limit (25 MB)";
                    break;
                case SkippReasonEnum.Cancelled:
                    result.OperationStatus = "Cancelled by user";
                    break;
                case SkippReasonEnum.Failed:
                    result.OperationStatus = reason;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("skippReasonEnum", skippReasonEnum, null);
            }
            return result;
        }

        public float? MalwareProbability { get; set; }

        public DateTime? FirstAnalysisDate { get; set; }

        public ErrorCodesEnum ReturnCode { get; set; }

        public string OperationStatus { get; set; }
    }
}
