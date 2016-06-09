

using System;
using DTO = Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core.Valkyrie
{
    public static class MapperHelper
    {
        public static ValkyrieVerdict ToBusiness(this DTO.ValkyrieVerdict verdict)
        {
            switch (verdict)
            {
                case DTO.ValkyrieVerdict.Clean:
                    return ValkyrieVerdict.Clean;
                case DTO.ValkyrieVerdict.Malware:
                    return ValkyrieVerdict.Malware;
                case DTO.ValkyrieVerdict.NotAnalyzed:
                    return ValkyrieVerdict.NotAnalyzed;
                case DTO.ValkyrieVerdict.PotentiallyUnwantedApplication:
                    return ValkyrieVerdict.PotentiallyUnwantedApplication;
                case DTO.ValkyrieVerdict.Undetected:
                    return ValkyrieVerdict.Undetected;
                case DTO.ValkyrieVerdict.FirstSeen:
                    return ValkyrieVerdict.FirstSeen;
                default:
                    throw new ArgumentException("unsupported Valkyrie verdict");
            }
        }
    }
}
