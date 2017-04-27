using System;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;
using Valkyrie.WebApiProvider.Enums;
using ValkyrieVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict;

namespace APTAT.Core.Reports
{
    public class DetailedBadFileWithValkyrie : DetailedBadFile
    {
        private readonly IFinalVerdictCalculator _calculator;
        public DetailedBadFileWithValkyrie(IModule module, ScannedTarget target, IFinalVerdictCalculator calculator, IValkyrieAnalysisResult analysisResult)
            : base(module, target)
        {
            _calculator = calculator;
            SetValkyrieAnalysis(analysisResult);
        }
        private void SetValkyrieAnalysis(IValkyrieAnalysisResult analysisResult)
        {
            if (analysisResult == null)
                return;
            IsWhiteListed = analysisResult.IsWhiteListed;
            MalwareProbability = analysisResult.MalwareProbability;
            ReturnCode = analysisResult.ReturnCode;
            Verdict = analysisResult.Verdict;
            IsSkipped = analysisResult.IsSkipped;
            SkipReason = analysisResult.SkipReason;
            OperationStatus = analysisResult.OperationStatus;
        }
        public string ValkyrieVerdictString
        {
            get
            {
                string result = this.Verdict.GetEnumDescription();
                if (this.IsSkipped)
                {
                    if (!String.IsNullOrWhiteSpace(this.OperationStatus))
                    {
                        if (OperationStatus.Length > 80)
                        {
                            var s = OperationStatus.Substring(0, 80);
                            s = s.Substring(0, s.LastIndexOf(" "));
                            return $"{result} ({s})";
                        }
                        result += " (" + OperationStatus + ")";
                    }
                }

                return result;
            }
        }
        public float? MalwareProbability { get; set; }

        public string MalwareProbabilityFormatted => $"{MalwareProbability / 100:P}";

        public ErrorCodesEnum ReturnCode { get; set; }

        public ValkyrieVerdict Verdict { get; set; }

        public FinalVerdict FinalVerdict => _calculator.GetFinalVerdict(this.Cloud, this.Verdict);

        public string FinalVerdictString => _calculator.GetVerdictString(this.FinalVerdict, this.OperationStatus);

        public bool IsSkipped { get; set; }

        public string SkipReason { get; set; }

        public string OperationStatus { get; set; }

        public bool IsWhiteListed { get; set; }

        public string WhiteListed => IsWhiteListed ? "Yes" : "No";

        public override BadFileType FileType => GetBadFileTypeFromVerdict();

        private BadFileType GetBadFileTypeFromVerdict()
        {
            switch (this.FinalVerdict)
            {
                case Core.FinalVerdict.Malware:
                    return BadFileType.Infected;
                case Core.FinalVerdict.Unknown:
                    return BadFileType.Unknown;
                case Core.FinalVerdict.NotAnalyzed:
                    return BadFileType.NotAnalyzed;
                case Core.FinalVerdict.InProgress:
                    return BadFileType.Pending;
                default:
                    throw new ArgumentOutOfRangeException("FinalVerdict");

            }
        }
    }
}