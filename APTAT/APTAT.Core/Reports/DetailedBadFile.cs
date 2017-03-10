using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using Valkyrie.WebApiProvider.Enums;
using ValkyrieVerdict = APTAT.Valkyrie.ValkyrieVerdict;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Core.Reports
{
    public class DetailedBadFile
    {
        private readonly List<ScannedTarget> _targets = new List<ScannedTarget>();
        private readonly IFinalVerdictCalculator _finalVerdictCalculator = FinalVerdictCalculator.GetInstance();

        public DetailedBadFile(IModule module, ScannedTarget target)
        {
            Verdict = ValkyrieVerdict.NotAnalyzed;
            if (module != null)
            {
                Sha1 = module.Sha1;
                Name = System.IO.Path.GetFileName(module.Path);
                Path = module.Path;
                Size = module.Size;
                Signer = module.Signer.TrimTo();
                Version = module.Version;
                Copyright = module.Copyright;
                Product = module.Product.TrimTo();
                Cloud = module.Cloud;
                FileSizeFormatted = module.FileSizeFormatted;
                Created = module.Created;
                Accessed = module.Accessed;
                Modified = module.Modified;
                FileAge = module.FileAge;
            }

            target.BadFileInstances.Add(new BadFileInstanceInfo(module));
            _targets.Add(target);
        }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }

        public string Sha1 { get; set; }
        public string Name { get; set; }

        public long Size { get; set; }
        public string Signer { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public Descent.Apt.CloudVerdict Cloud { get; set; }
        public string FileSizeFormatted { get; set; }

        public BadFileType FileType
        {
            get
            {
                if (Verdict == ValkyrieVerdict.FirstSeen || Verdict == ValkyrieVerdict.Pending)
                {
                    return BadFileType.Pending;
                }

                if (Verdict == ValkyrieVerdict.NotAnalyzed)
                {
                    if (Cloud == CloudVerdict.NoVerdict || Cloud == CloudVerdict.FailedToAnalyze)
                        return BadFileType.NotAnalyzed;

                    return Cloud == CloudVerdict.Unknown || Cloud == CloudVerdict.FirstSeen ? BadFileType.Unknown : BadFileType.Infected;
                }
                return Verdict == ValkyrieVerdict.Undetected ? BadFileType.Unknown : BadFileType.Infected;
            }
        }

        public void SetValkyrieAnalysis(ValkyrieAnalysisResult analysisResult)
        {
            if (analysisResult == null)
                return;
            IsWhiteListed = analysisResult.IsWhiteListed;
            MalwareProbability = analysisResult.MalwareProbability;
            ReturnCode = analysisResult.ReturnCode;
            Verdict = analysisResult.Verdict;
            IsWhiteListed = analysisResult.IsWhiteListed;
            IsSkipped = analysisResult.IsSkipped;
            SkipReason = analysisResult.SkipReason;
            OperationStatus = analysisResult.OperationStatus;
            FinalVerdict = _finalVerdictCalculator.GetFinalVerdict(this.Cloud, analysisResult.Verdict);
            FinalVerdictString = _finalVerdictCalculator.GetVerdictString(this.FinalVerdict);
        }



        public int TargetsCount
        {
            get { return Targets.Count; }
        }

        public string CloudString
        {
            get
            {
                switch (Cloud)
                {
                    case CloudVerdict.Safe:
                        return "Trusted";
                    case CloudVerdict.Unknown:
                        return "Unknown";
                    case CloudVerdict.Malware:
                        return "Malware";
                    case CloudVerdict.FirstSeen:
                        return "Unknown (FirstSeen)";
                    case CloudVerdict.FailedToAnalyze:
                    case CloudVerdict.NoVerdict:
                        return "Lookup in Progress";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ;
            }
        }


        public List<ScannedTarget> Targets
        {
            get { return _targets; }
        }

        public int InstancesTotalCount
        {
            get
            {
                return _targets.Sum(t => t.BadFileInstances.Count);
            }
        }
        public class BadFileInstanceInfo
        {
            public BadFileInstanceInfo()
            {
            }

            public BadFileInstanceInfo(IModule module)
            {
                if (module != null)
                {
                    Id = module.Id;
                    Path = module.Path;
                    Created = module.Created;
                    Modified = module.Modified;
                    Accessed = module.Accessed;
                }
            }

            public string Id { get; set; }
            public string Path { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public DateTime Accessed { get; set; }
        }

        public bool IsWhiteListed { get; set; }

        public string WhiteListed
        {
            get { return IsWhiteListed ? "Yes" : "No"; }
        }

        public float? MalwareProbability { get; set; }

        public string MalwareProbabilityFormatted
        {
            get { return string.Format("{0:P}", MalwareProbability / 100); }
        }

        public ErrorCodesEnum ReturnCode { get; set; }

        public ValkyrieVerdict Verdict { get; set; }

        public FinalVerdict FinalVerdict { get; private set; }
        public string FinalVerdictString { get; private set; }
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
                            return result += " (" + s + ")";
                        }
                        result += " (" + this.OperationStatus + ")";
                    }
                }

                return result;
            }
        }

        public bool IsSkipped { get; set; }

        public string SkipReason { get; set; }

        public string OperationStatus { get; set; }

        public string FileAge { get; set; }
    }
}