using System;

namespace APTAT.Core.CMC
{
    class SystemInfoProvider : ISystemInfoProvider
    {
        private SystemInfo _systemInfo;

        public SystemInfoProvider(ISystemInfo systemInfo)
        {
            _systemInfo = new SystemInfo()
            {
                FamilyName = systemInfo.FamilyName,
                Platform = systemInfo.Platform,
                Version = systemInfo.Platform
            };
        }

        public ISystemInfo GetSystemInfo()
        {
            return _systemInfo;
        }
    }
}