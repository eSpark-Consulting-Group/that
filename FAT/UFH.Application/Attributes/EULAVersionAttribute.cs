using System;
using System.Linq;
using System.Reflection;

namespace UFH.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class EULAVersionAttribute : Attribute
    {
        private string _version;
        public EULAVersionAttribute() : this(string.Empty) { }
        public EULAVersionAttribute(string version)
        {
            _version = version;
        }

        public string Version
        {
            get { return _version; }
        }
    }

    public static class AttributeExtensions
    {
        public static string GetEULAVersionAttribute(this Assembly assembly)
        {
            var attr = assembly.GetCustomAttributes(typeof(EULAVersionAttribute), false).
                            OfType<EULAVersionAttribute>().FirstOrDefault();
            if (attr != null)
                return attr.Version;

            return null;
        }
    }
}
