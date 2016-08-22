using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using APTAT.DeploymentCore.Messages;

namespace APTAT.Core
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
