using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;
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
                        var val = new ValkyrieFileInfo(badFile.Sha1, scanResult.Computer, badFile.Path, 
                            badFile.Size, badFile.Cloud, ValkyrieVerdict.InQueue);
                        val.AddFullPath(scanResult.Computer, badFile.Path);

                        files.Add(badFile.Sha1, val);
                    }
                    else
                    {
                        files[badFile.Sha1].AddFullPath(scanResult.Computer, badFile.Path);
                    }
                }
            }
            return files;
        }
    }
}
