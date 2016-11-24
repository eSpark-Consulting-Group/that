using System;
using System.ComponentModel;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Valkyrie
{
    public class ValkyrieAnalysisResult
    {
        public static ValkyrieAnalysisResult Create(BasicInfoResponse basicInfo)
        {
            var result = new ValkyrieAnalysisResult(basicInfo.SHA1, basicInfo.Verdict.ToBusiness())
            {
                FirstAnalysisDate = basicInfo.FirstAnalysisDate,
                LastAnalysisDate = basicInfo.LastAnalysisDate,
                MalwareProbability = basicInfo.MalwareProbability,
                Verdict = basicInfo.Verdict.ToBusiness(),
                IsWhiteListed = basicInfo.IsWhiteListed,
                ReturnCode = basicInfo.ReturnCode
            };
            
            result.OperationStatus = result.ReturnCode.ToString();
          
            return result;
        }

        public static ValkyrieAnalysisResult Create(BasicInfoResponse basicInfo, string analyseId, bool submitted)
        {
            var result = new ValkyrieAnalysisResult(basicInfo.SHA1, ValkyrieVerdict.Pending)
            {
                AnalyzeId = analyseId,
                ReturnCode = ErrorCodesEnum.Success,
                IsSubmitted = submitted
            };
            result.OperationStatus = result.ReturnCode.ToString();
            return result;
        }

        public static ValkyrieAnalysisResult Create(CheckAnalysisResultResponse automaticResponse)
        {
            ValkyrieAnalysisResult result = new ValkyrieAnalysisResult(automaticResponse.SHA1, 
                automaticResponse.Verdict.ToBusiness())
            {
                FirstAnalysisDate = automaticResponse.FirstAnalysisDate,
                LastAnalysisDate = automaticResponse.LastAnalysisDate,
                IsWhiteListed = automaticResponse.IsWhiteListed,
                MalwareProbability = automaticResponse.MalwareProbability,
                ReturnCode = automaticResponse.ReturnCode
            };
            result.OperationStatus = result.ReturnCode.ToString();
            return result;
        }

        public ValkyrieAnalysisResult(ValkyrieAnalysisResult valkyrie)
        {
            AnalyzeId = valkyrie.AnalyzeId;
            SHA1 = valkyrie.SHA1;

        }

        public ValkyrieAnalysisResult(string Sha1, ValkyrieVerdict verdict)
        {
            SHA1 = Sha1;
            Verdict = verdict;
        }
        private ValkyrieAnalysisResult()
        { }

        public string AnalyzeId { get; private set; }
        public string SHA1 { get; }
        public bool IsWhiteListed { get; private set; }
        public bool IsSubmitted { get; private set; }
        public DateTime? LastAnalysisDate { get; private set; }
        public ValkyrieVerdict Verdict { get; private set; }
        public float? MalwareProbability { get; private set; }
        public DateTime? FirstAnalysisDate { get; private set; }
        public ErrorCodesEnum ReturnCode { get; private set; }
        public string OperationStatus { get; private set; }
        public bool IsSkipped { get; private set; }
        public string SkipReason { get; private set; }
        public SkippReasonEnum? SkipReasonAsEnum { get; private set; }

        public string VerdictString
        {
            get
            {
                switch (Verdict)
                {
                    case ValkyrieVerdict.NotAnalyzed:
                        return $"Not Analyzed ({OperationStatus.Replace(Environment.NewLine, "")})";
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                        return "Potentially Unwanted";
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        return "In Analysis";
                    case ValkyrieVerdict.Undetected:
                        return "No Threat Found";
                    case ValkyrieVerdict.PendingFls:
                        return "Lookup in Progress";
                }
                return Verdict.ToString();
            }
        }



        public static ValkyrieAnalysisResult CreateSkipInfo(string sha1, string reason, SkippReasonEnum skippReasonEnum)
        {
            var result = new ValkyrieAnalysisResult(sha1, ValkyrieVerdict.NotAnalyzed) {IsSkipped = true,
                SkipReason = reason, IsWhiteListed = false, LastAnalysisDate = null};
            switch (skippReasonEnum)
            {
                case SkippReasonEnum.Skipped:
                    result.OperationStatus = "Exceed size limit (25 MB)";
                    break;
                case SkippReasonEnum.Cancelled:
                    result.OperationStatus = "Cancelled by user";
                    break;
                case SkippReasonEnum.WebException:
                    result.Verdict = ValkyrieVerdict.Pending;
                    result.OperationStatus = reason;
                    break;
                case SkippReasonEnum.FileNotFound:
                    result.OperationStatus = reason;
                    break;
                case SkippReasonEnum.Failed:
                    result.OperationStatus = reason;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("skippReasonEnum", skippReasonEnum, null);
            }
            return result;
        }
    }

    public enum ValkyrieVerdict
    {
        Clean,

        [Description("No Threat Found")]
        Undetected,

        [Description("In Analysis")]
        FirstSeen,

        Malware,

        [Description("Potentially Unwanted Application")]
        PotentiallyUnwantedApplication,
        
        [Description("Not Analyzed")]
        NotAnalyzed,

        [Description("In Analysis")]
        Pending,

        [Description("Lookup in Progress")]
        PendingFls
    }
}
