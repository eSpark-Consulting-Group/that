using System.Globalization;
using System.Reflection;

namespace APTAT.DeploymentCore.Messages
{
    public class ExtendedResourceManager : System.Resources.ResourceManager
    {
        public ExtendedResourceManager(string baseName, Assembly assembly)
            : base(baseName, assembly)
        { }


        public string GetDefaultString(string name)
        {
            return GetString(name, CultureInfo.InvariantCulture);
        }


        public ResourceText GetResourceText(string sourceName, string resourceId)
        {
            string defaultString = GetDefaultString(resourceId);

            return new ResourceText(sourceName, resourceId, defaultString ?? string.Empty);
        }
    }
}
