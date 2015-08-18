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
            result.LastAnalysisDate = basicInfo.LastAnalysisDate;
            result.Verdict = basicInfo.Verdict;
            result.IsWhiteListed = basicInfo.IsWhiteListed;
            return result;
        }

        public static AnalysisResult Create(SubmitToAutomaticResponse automaticResponse)
        {
            AnalysisResult result = new AnalysisResult();
            result.SHA1 = automaticResponse.SHA1;
            result.LastAnalysisDate = automaticResponse.LastAnalysisDate;
            result.Verdict = automaticResponse.Verdict;
            result.IsWhiteListed = automaticResponse.IsWhiteListed;
            return result;
        }

        public string SHA1 { get; private set; }
        public string FilePath { get; private set; }
        public bool IsWhiteListed { get; private set; }
        public DateTime? LastAnalysisDate { get; private set; }
        public InfoVerdictEnum Verdict { get; private set; }
        public bool IsSkipped { get; private set; }
        public string SkipReason { get; private set; }

        internal static AnalysisResult CreateSkipInfo(string reason)
        {
            return new AnalysisResult() { IsSkipped = true, SkipReason = reason };
        }
    }
}
