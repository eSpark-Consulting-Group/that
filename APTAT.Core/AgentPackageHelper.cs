using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core
{
    public interface IAgentPackageHelper
    {
        bool GenerateInstaller(ManagedProduct agentProduct);

        bool GenerateInstaller(string relativePath, ManagedProduct agentProduct, ManagedProduct dependentProduct);

        string GetManagedProductInstallerName(ManagedProduct product);

        Package GetAppropriatePackage(ManagedProduct product, SystemKind systemKind);

        string GetPackageVersion(string packagePath);

        string PackagesFolder { get; }

        string InstallerName { get; }

        SystemKind AgentKind { get; }

        Guid PluginUid { get; }
    }

    public class WindowsAgentPackageHelper : IAgentPackageHelper
    {
        public WindowsAgentPackageHelper(string defaultRoot, ushort socketPortNumber, byte[] publicKey)
        {
            
        }

        public bool GenerateInstaller(ManagedProduct agentProduct)
        {
            throw new NotImplementedException();
        }

        public bool GenerateInstaller(string relativePath, ManagedProduct agentProduct, ManagedProduct dependentProduct)
        {
            throw new NotImplementedException();
        }

        public string GetManagedProductInstallerName(ManagedProduct product)
        {
            throw new NotImplementedException();
        }

        public Package GetAppropriatePackage(ManagedProduct product, SystemKind systemKind)
        {
            throw new NotImplementedException();
        }

        public string GetPackageVersion(string packagePath)
        {
            throw new NotImplementedException();
        }

        public string PackagesFolder
        {
            get { throw new NotImplementedException(); }
        }

        public string InstallerName
        {
            get { throw new NotImplementedException(); }
        }

        public SystemKind AgentKind
        {
            get { throw new NotImplementedException(); }
        }

        public Guid PluginUid
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class DebianAgentPackageHelper : IAgentPackageHelper
    {
        public DebianAgentPackageHelper(string defaultRoot, ushort socketPortNumber, byte[] publicKey)
        {
            
        }

        public bool GenerateInstaller(ManagedProduct agentProduct)
        {
            throw new NotImplementedException();
        }

        public bool GenerateInstaller(string relativePath, ManagedProduct agentProduct, ManagedProduct dependentProduct)
        {
            throw new NotImplementedException();
        }

        public string GetManagedProductInstallerName(ManagedProduct product)
        {
            throw new NotImplementedException();
        }

        public Package GetAppropriatePackage(ManagedProduct product, SystemKind systemKind)
        {
            throw new NotImplementedException();
        }

        public string GetPackageVersion(string packagePath)
        {
            throw new NotImplementedException();
        }

        public string PackagesFolder
        {
            get { throw new NotImplementedException(); }
        }

        public string InstallerName
        {
            get { throw new NotImplementedException(); }
        }

        public SystemKind AgentKind
        {
            get { throw new NotImplementedException(); }
        }

        public Guid PluginUid
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class MacOSAgentPackageHelper : IAgentPackageHelper
    {
        public MacOSAgentPackageHelper(string defaultRoot, ushort socketPortNumber, byte[] publicKey)
        {
            
        }

        public bool GenerateInstaller(ManagedProduct agentProduct)
        {
            throw new NotImplementedException();
        }

        public bool GenerateInstaller(string relativePath, ManagedProduct agentProduct, ManagedProduct dependentProduct)
        {
            throw new NotImplementedException();
        }

        public string GetManagedProductInstallerName(ManagedProduct product)
        {
            throw new NotImplementedException();
        }

        public Package GetAppropriatePackage(ManagedProduct product, SystemKind systemKind)
        {
            throw new NotImplementedException();
        }

        public string GetPackageVersion(string packagePath)
        {
            throw new NotImplementedException();
        }

        public string PackagesFolder
        {
            get { throw new NotImplementedException(); }
        }

        public string InstallerName
        {
            get { throw new NotImplementedException(); }
        }

        public SystemKind AgentKind
        {
            get { throw new NotImplementedException(); }
        }

        public Guid PluginUid
        {
            get { throw new NotImplementedException(); }
        }
    }
}
