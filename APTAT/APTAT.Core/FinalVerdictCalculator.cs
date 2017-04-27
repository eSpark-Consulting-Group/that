using APTAT.Core.Descent.Apt;
using System;

namespace APTAT.Core
{
    public enum FinalVerdict
    {
        Clean = 1,
        Malware,
        Unknown,
        NotAnalyzed,
        InProgress
    }

    public class FinalVerdictCalculator : IFinalVerdictCalculator
    {

        public FinalVerdictCalculator() { }
        public FinalVerdict GetFinalVerdict(CloudVerdict cloud, APTAT.Core.Valkyrie.ValkyrieVerdict valkyrie)
        {
            if (cloud == CloudVerdict.Malware)
                return FinalVerdict.Malware;
            if (cloud == CloudVerdict.Safe)
                return FinalVerdict.Clean;
            switch (valkyrie)
            {
                case APTAT.Core.Valkyrie.ValkyrieVerdict.NotAnalyzed:
                    return FinalVerdict.NotAnalyzed;
                case APTAT.Core.Valkyrie.ValkyrieVerdict.PotentiallyUnwantedApplication:
                case APTAT.Core.Valkyrie.ValkyrieVerdict.Malware:
                    return FinalVerdict.Malware;
                case APTAT.Core.Valkyrie.ValkyrieVerdict.Pending:
                case APTAT.Core.Valkyrie.ValkyrieVerdict.FirstSeen:
                    return FinalVerdict.InProgress;
                case APTAT.Core.Valkyrie.ValkyrieVerdict.Undetected:
                    return FinalVerdict.Unknown;
                case APTAT.Core.Valkyrie.ValkyrieVerdict.Clean:
                    return FinalVerdict.Clean;
                case APTAT.Core.Valkyrie.ValkyrieVerdict.PendingFls:
                case APTAT.Core.Valkyrie.ValkyrieVerdict.InQueue:
                    {
                        switch (cloud)
                        {
                            case CloudVerdict.Safe:
                                return FinalVerdict.Clean;
                            case CloudVerdict.FirstSeen:
                            case CloudVerdict.FailedToAnalyze:
                            case CloudVerdict.NoVerdict:
                            case CloudVerdict.Unknown:
                                return FinalVerdict.InProgress;
                            case CloudVerdict.Malware:
                                return FinalVerdict.Malware;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(CloudVerdict));
                        }
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(valkyrie));
            }
        }


        public string GetVerdictString(FinalVerdict FinalVerdict, string operationStatus = null)
        {

            switch (FinalVerdict)
            {
                case FinalVerdict.Clean:
                    return "Clean";
                case FinalVerdict.InProgress:
                    return "In Analysis";
                case FinalVerdict.Malware:
                    return "Malware";
                case FinalVerdict.NotAnalyzed:
                    return $"Not Analyzed  ({operationStatus?.Replace(Environment.NewLine, "")})";
                case FinalVerdict.Unknown:
                    return "No Threat Found";
                default:
                    throw new ArgumentOutOfRangeException(nameof(FinalVerdict));
            }
        }
    }
}
