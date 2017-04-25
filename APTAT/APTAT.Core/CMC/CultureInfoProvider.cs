using System.Globalization;

namespace APTAT.Core.CMC
{
    public class CultureInfoProvider : ICultureInfoProvider
    {
        public string GetCulture()
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }
}