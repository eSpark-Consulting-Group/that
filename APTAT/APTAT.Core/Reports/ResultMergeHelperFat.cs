using System;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;

namespace APTAT.Core.Reports
{
    public class ResultMergeHelperFat : ResultMergeHelperValkyrie
    {
        public ResultMergeHelperFat(IFinalVerdictCalculator calc)
            : base(calc)
        {

        }
       
        private static void UpdateStatisticsByFinalVerdict(Statistics computerStatistics, FinalVerdict final)
        {
            switch (final)
            {
                case FinalVerdict.Clean:
                    computerStatistics.MergedSafeCount++;
                    break;
                case FinalVerdict.Malware:
                    computerStatistics.MergedMalwareCount++;
                    break;
                case FinalVerdict.Unknown:
                    computerStatistics.MergedUnknownCount++;
                    break;
                case FinalVerdict.NotAnalyzed:
                    computerStatistics.MergedErrorFilesCount++;
                    break;
                case FinalVerdict.InProgress:
                    computerStatistics.MergedPendingFilesCount++;
                    break;
                default:
                    throw new NotSupportedException(nameof(final));
                    
            }
        }

        protected override void CalculateStatistics(IModule module, Statistics computerStatistics)
        {
            CalculateStatistics(module, computerStatistics, ValkyrieVerdict.InQueue);
        }

   
        protected override void CalculateStatistics(IModule module, Statistics computerStatistics, ValkyrieVerdict valkyrieVerdict)
        {
            var final = Calculator.GetFinalVerdict(module.Cloud, valkyrieVerdict);
            UpdateStatisticsByFinalVerdict(computerStatistics, final);
        }
    }
}