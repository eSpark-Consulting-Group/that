using System;
using System.ComponentModel;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieAnalysisResult : IValkyrieAnalysisResult
    {
        public static ValkyrieAnalysisResult Create(IBasicInfoResponse basicInfo)
        {
            var result = new ValkyrieAnalysisResult(basicInfo.SHA1, basicInfo.Verdict.ToBusiness())
            {
                FirstAnalysisDate = basicInfo.FirstAnalysisDate,
                LastAnalysisDate = basicInfo.LastAnalysisDate,
                MalwareProbability = basicInfo.MalwareProbability,
                IsWhiteListed = basicInfo.IsWhiteListed,
                ReturnCode = basicInfo.ReturnCode,
            };
            
            result.OperationStatus = result.ReturnCode.ToString();
          
            return result;
        }

        public static ValkyrieAnalysisResult Create(IBasicInfoResponse basicInfo, string analyseId, bool submitted)
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


        public ValkyrieAnalysisResult(string sha1, ValkyrieVerdict verdict)
        {
            Sha1 = sha1;
            Verdict = verdict;
            AnalysisDate = DateTime.UtcNow;
        }

        public DateTime AnalysisDate { get; }
        public string AnalyzeId { get; private set; }
        public string Sha1 { get; }
        public bool IsWhiteListed { get; private set; }
        public bool IsSubmitted { get; private set; }
        public DateTime? LastAnalysisDate { get; private set; }
        public ValkyrieVerdict Verdict { get; }
        public float? MalwareProbability { get; private set; }
        public DateTime? FirstAnalysisDate { get; private set; }
        public ErrorCodesEnum ReturnCode { get; private set; }
        public string OperationStatus { get; private set; }
        public bool IsSkipped { get; private set; }
        public string SkipReason { get; private set; }
        
        public SkippReasonEnum SkipReasonEnum { get; internal set; }

        public static ValkyrieAnalysisResult CreateSkipInfo(string sha1, string reason, SkippReasonEnum skippReasonEnum)
        {
            var verdict = ValkyrieVerdict.NotAnalyzed;
            string operationStatus = null;
            switch (skippReasonEnum)
            {
                case SkippReasonEnum.Skipped:
                    operationStatus = "Exceed size limit (25 MB)";
                    break;
                case SkippReasonEnum.Cancelled:
                    operationStatus = "Cancelled by user";
                    break;
                case SkippReasonEnum.WaitNextInterval:
                    verdict = ValkyrieVerdict.Pending;
                    break;
                case SkippReasonEnum.WebException:
                    verdict = ValkyrieVerdict.Pending;
                    operationStatus = reason;
                    break;
                case SkippReasonEnum.FileNotFound:
                    operationStatus = reason;
                    break;
                case SkippReasonEnum.Failed:
                    operationStatus = reason;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skippReasonEnum), skippReasonEnum, null);
            }
            var result = new ValkyrieAnalysisResult(sha1, verdict)
            {
                IsSkipped = true,
                SkipReason = reason,
                SkipReasonEnum = skippReasonEnum,
                IsWhiteListed = false,
                LastAnalysisDate = null,
                OperationStatus = operationStatus
            };
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

        [Description("In Analysis")]
        InQueue,

        [Description("Lookup in Progress")]
        PendingFls
    }
}
