using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;
using System;
using System.Collections.Generic;
using System.Linq;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;


namespace APTAT.Core
{
    public class FatFileFilter : DistinctFileHelper
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

        public List<IValkyrieFileInfo> GetDistinctFileInfo(IAptScanResult[] results)
        {            
            return GetDistinctFileInfoDict(results).Values.ToList();
        }

        public Dictionary<string, IValkyrieFileInfo> GetDistinctFileInfoDict(IAptScanResult[] results)
        {
            IAptScanResult[] scanResults = results.OrderByDescending(r => r.Computer.IsLocalComputer)
                .ToArray();

            Dictionary<string, IValkyrieFileInfo> files = new Dictionary<string, IValkyrieFileInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var scanResult in scanResults)
            {
                var modules = scanResult.Files.Where(ShouldSendToValkyrie);

                foreach (var badFile in modules)
                {
                    if (!files.ContainsKey(badFile.Sha1))
                    {
                        var val = new ValkyrieFileInfo(badFile.Sha1, scanResult.Computer.IPAddress, badFile.Path, 
                            badFile.Size, MapCloud(badFile.Cloud), scanResult.Computer.IsLocalComputer, ValkyrieVerdict.InQueue);
                        val.AddFullPath(scanResult.Computer.IPAddress, badFile.Path);

                        files.Add(badFile.Sha1, val);
                    }
                    else
                    {
                        files[badFile.Sha1].AddFullPath(scanResult.Computer.IPAddress, badFile.Path);
                    }
                }
            }
            return files;
        }
        private static APTAT.Valkyrie.CloudVerdict MapCloud(CloudVerdict cloud)
        {
            switch (cloud)
            {
                case CloudVerdict.Safe:
                    return APTAT.Valkyrie.CloudVerdict.Safe;
                case CloudVerdict.Unknown:
                    return APTAT.Valkyrie.CloudVerdict.Unknown;

                case CloudVerdict.Malware:
                    return APTAT.Valkyrie.CloudVerdict.Malware;

                case CloudVerdict.FirstSeen:
                    return APTAT.Valkyrie.CloudVerdict.FirstSeen;

                case CloudVerdict.FailedToAnalyze:
                    return APTAT.Valkyrie.CloudVerdict.FailedToAnalyze;

                case CloudVerdict.NoVerdict:
                    return APTAT.Valkyrie.CloudVerdict.NoVerdict;

                default:
                    throw new NotSupportedException("Can not convert to Valkyrie.CloudVerdict from Apt.CloudVerdict " +
                                                    cloud);
            }
        }
    }
}
