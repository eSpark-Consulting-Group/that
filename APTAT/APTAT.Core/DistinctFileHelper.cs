using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using System;
using System.Collections.Generic;
using System.Linq;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;


namespace APTAT.Core
{
    public class FATFileFilter : DistinctFileHelper
    {
        public override bool ShouldSendToValkyrie(IFlsResult module)
        {
            return module.Cloud == CloudVerdict.Unknown || module.Cloud == CloudVerdict.FirstSeen || module.Cloud == CloudVerdict.Malware;
        }
    }


    public class DistinctFileHelper : IDistinctFileHelper
    {
        public virtual bool ShouldSendToValkyrie(IFlsResult module)
        {
            return module.Cloud == CloudVerdict.Unknown || module.Cloud == CloudVerdict.FirstSeen;
        }

        public List<ValkyrieFileInfo> GetDistinctFileInfo(IAptScanResult[] results)
        {
            IAptScanResult[] scanResults = results.OrderByDescending(r => r.Computer.IsLocalComputer)
                .ToArray();

            Dictionary<string, ValkyrieFileInfo> files = new Dictionary<string, ValkyrieFileInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var scanResult in scanResults)
            {
                var modules = scanResult.Files.Where(ShouldSendToValkyrie);

                foreach (var badFile in modules)
                {
                    if (!files.ContainsKey(badFile.Sha1))
                    {
                        var val = new ValkyrieFileInfo(badFile.Sha1)
                        {
                            Ip = scanResult.Computer.IPAddress,
                            Path = badFile.Path,
                            Size = badFile.Size,
                            FlsCloud = MapCloud(badFile.Cloud),
                            IsLocalComputer = scanResult.Computer.IsLocalComputer
                        };
                        val.AddFullPath(scanResult.Computer.IPAddress, badFile.Path);

                        files.Add(badFile.Sha1, val);
                    }
                    else
                    {
                        files[badFile.Sha1].AddFullPath(scanResult.Computer.IPAddress, badFile.Path);
                    }
                }
            }
            return files.Values.ToList();
        }

        private static Valkyrie.CloudVerdict MapCloud(Descent.Apt.CloudVerdict cloud)
        {
            switch (cloud)
            {
                case Descent.Apt.CloudVerdict.Safe:
                    return Valkyrie.CloudVerdict.Safe;
                case Descent.Apt.CloudVerdict.Unknown:
                    return Valkyrie.CloudVerdict.Unknown;

                case Descent.Apt.CloudVerdict.Malware:
                    return Valkyrie.CloudVerdict.Malware;

                case Descent.Apt.CloudVerdict.FirstSeen:
                    return Valkyrie.CloudVerdict.FirstSeen;

                case Descent.Apt.CloudVerdict.FailedToAnalyze:
                    return Valkyrie.CloudVerdict.FailedToAnalyze;

                case Descent.Apt.CloudVerdict.NoVerdict:
                    return Valkyrie.CloudVerdict.NoVerdict;

                default:
                    throw new NotSupportedException("Can not convert to Valkyrie.CloudVerdict from Apt.CloudVerdict " +
                                                    cloud);
            }
        }
    }
}
