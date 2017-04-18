using System.Globalization;

namespace APTAT.Core.CMC
{
    class CultureInfoProvider : ICultureInfoProvider
    {
        public string GetCulture()
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }
}