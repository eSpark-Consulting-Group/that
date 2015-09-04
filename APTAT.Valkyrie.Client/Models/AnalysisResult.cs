using APTAT.Valkyrie.Client.Contracts;
using APTAT.Valkyrie.Client.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client
{
    public class AnalysisResult
    {
        public static AnalysisResult Create(BasicInfoResponse basicInfo)
        {
            AnalysisResult result = new AnalysisResult();
            result.SHA1 = basicInfo.SHA1;
            result.FirstAnalysisDate = basicInfo.FirstAnalysisDate;
            result.LastAnalysisDate = basicInfo.LastAnalysisDate;
            result.MalwareProbability = basicInfo.MalwareProbability;            
            result.Verdict = basicInfo.Verdict;
            result.IsWhiteListed = basicInfo.IsWhiteListed;
            result.ReturnCode = basicInfo.ReturnCode;
            return result;
        }

        public static AnalysisResult Create(SubmitToAutomaticResponse automaticResponse)
        {
            AnalysisResult result = new AnalysisResult();
            result.SHA1 = automaticResponse.SHA1;
            result.FirstAnalysisDate = automaticResponse.FirstAnalysisDate;
            result.LastAnalysisDate = automaticResponse.LastAnalysisDate;
            result.Verdict = automaticResponse.Verdict;
            result.IsWhiteListed = automaticResponse.IsWhiteListed;
            result.MalwareProbability = automaticResponse.MalwareProbability;
            result.ReturnCode = automaticResponse.ReturnCode;
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
                return Verdict == InfoVerdictEnum.Undetected ? "Threat Not Found" : Verdict.ToString();
            }
        }
        public bool IsSkipped { get; private set; }
        public string SkipReason { get; private set; }

        internal static AnalysisResult CreateSkipInfo(string sha1, string reason)
        {
            return new AnalysisResult() {IsSkipped = true, SHA1 = sha1, SkipReason = reason, Verdict = InfoVerdictEnum.Skipped, IsWhiteListed = false, LastAnalysisDate = null};
        }

        public float MalwareProbability { get; set; }

        public DateTime? FirstAnalysisDate { get; set; }

        public ErrorCodesEnum ReturnCode { get; set; }
    }
}
