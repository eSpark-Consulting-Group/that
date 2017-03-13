using System;
using APTAT.Valkyrie;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace FAT.Application.Models.Apt
{
    public class FatFileResult : BadFileItem, IFatFileResult
    {
        public ValkyrieVerdict? ValkyrieVerdictValue { get; set; }
        public float? MalwareProbabilityValue { get; set; }
        public string SkipReason { get; set; }
        public string OperationStatus { get; set; }
        public bool IsWhiteListed { get; set; }

        public UiFinalVerdict FinalVerdict
        {
            get
            {
                var valkyrieVerdictValue = ValkyrieVerdictValue ?? ValkyrieVerdict.InQueue;
                if (Cloud == CloudVerdict.Malware)
                    return UiFinalVerdict.Malware;
                if (Cloud == CloudVerdict.Safe)
                    return UiFinalVerdict.Clean;
                switch (valkyrieVerdictValue)
                {
                    case ValkyrieVerdict.NotAnalyzed:
                        return UiFinalVerdict.NotAnalyzed;
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                    case ValkyrieVerdict.Malware:
                        return UiFinalVerdict.Malware;
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        return UiFinalVerdict.InProgress;
                    case ValkyrieVerdict.Undetected:
                        return UiFinalVerdict.Unknown;
                    case ValkyrieVerdict.Clean:
                        return UiFinalVerdict.Clean;
                    case ValkyrieVerdict.PendingFls:
                    case ValkyrieVerdict.InQueue:
                        {
                            switch (Cloud)
                            {
                                case CloudVerdict.Safe:
                                    return UiFinalVerdict.Clean;
                                case CloudVerdict.FirstSeen:
                                case CloudVerdict.FailedToAnalyze:
                                case CloudVerdict.NoVerdict:
                                case CloudVerdict.Unknown:
                                    return UiFinalVerdict.InProgress;
                                case CloudVerdict.Malware:
                                    return UiFinalVerdict.Malware;

                                default:
                                    throw new ArgumentOutOfRangeException(nameof(CloudVerdict));
                            }
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ValkyrieVerdict));
                }
            }
        }

        public override bool IsUnknown => FinalVerdict == UiFinalVerdict.Unknown;

        public override bool IsMalware => FinalVerdict == UiFinalVerdict.Malware;

        public override bool IsClean => FinalVerdict == UiFinalVerdict.Clean;
    }
}
