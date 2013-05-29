using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core
{
    public static class PluginGuids
    {
        public static Guid CisGuid = Guid.Empty;
        public static Guid LinuxDebianAgentGuid = Guid.Empty;
        public static Guid MacOSXAgentGuid = Guid.Empty;
        public static Guid WindowsAgentGuid = Guid.Empty;
    }

    public static class ProductManagementTools
    {
        private const int ParameterizedCisVersion = 4;

        public static Version GetCisVersion(this IComputer computer)
        {
            if (computer == null)
            {
                throw new ArgumentNullException("computer");
            }

            return computer.GetManagedProductVersion(PluginGuids.CisGuid);
        }

        ///// <summary>
        ///// Convert product platform from string to enumeration
        ///// </summary>
        ///// <param name="productPlatform">Platform string (i.e. x86)</param>
        ///// <returns>Platform</returns>
        //public static ProductPlatform ParseProductPlatform(string productPlatform)
        //{
        //    ProductPlatform result = ProductPlatform.Unknown;

        //    if (!String.IsNullOrEmpty(productPlatform))
        //    {
        //        productPlatform = productPlatform.ToLowerInvariant();
        //        if (productPlatform == "intel" || productPlatform == "x86")
        //            result |= ProductPlatform.X86;
        //        else
        //            if (productPlatform == "amd64" || productPlatform == "x64")
        //                result |= ProductPlatform.AMD64;
        //            else
        //                if (productPlatform == "intel64")
        //                    result |= ProductPlatform.IA64;
        //    }
        //    else
        //    {
        //        // if platform is empty, like CDE, we can assume that it supports both platforms
        //        result |= ProductPlatform.X86;
        //        result |= ProductPlatform.AMD64;
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Detect package platform and managed product
        ///// </summary>
        ///// <param name="package"></param>
        //public static void ActualizePackage(Package package)
        //{
        //    // Detect platform
        //    string[] platforms = new[] { String.Empty };

        //    try
        //    {
        //        MsiTools.MsiSummary msiSummary;
        //        if (MsiTools.GetPackageSummary(package.StoredBinaryPath, out msiSummary))
        //        {
        //            package.LanguageIdentifiers = msiSummary.LanguageIdentifiers;
        //            platforms = msiSummary.Platforms;
        //        }
        //    }
        //    catch
        //    {
        //    }


        //    package.Platform = ProductPlatform.Unknown;
        //    foreach (string platform in platforms)
        //    {
        //        package.Platform |= ParseProductPlatform(platform);
        //    }
        //}

        ///// <summary>
        ///// Detect if managed product is installed on the target computer
        ///// </summary>
        ///// <param name="pc"></param>
        ///// <param name="pluginUid"></param>
        ///// <returns></returns>
        //public static bool IsProductInstalled(IComputer pc, Guid pluginUid)
        //{
        //    Version productVersion = pc.GetManagedProductVersion(pluginUid);
        //    return (productVersion != null);
        //}

        //public static bool IsLanguageParameterSupported(IPackage package)
        //{
        //    if (package.PluginUid == PluginGuids.CisGuid && new Version(package.ProductVersion).Major >= ParameterizedCisVersion)
        //        return true;

        //    return false;
        //}
    }
}
