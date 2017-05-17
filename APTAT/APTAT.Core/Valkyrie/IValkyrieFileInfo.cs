using System.Collections.ObjectModel;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using System.Collections.Generic;

namespace APTAT.Core.Valkyrie
{

    public interface IFileInfo
    {
        IComputer Computer { get; }
        string Path { get; }
        ReadOnlyCollection<IFileInstanceInformation> Instances { get; }
        string Sha1 { get; }
        //ICopyFileResult CopyResult { get; set; }
    }
    public interface IFileInstanceInformation
    {
        IComputer Computer { get; }
        string Path { get; }
    }

    public interface IValkyrieFileInfo : IFileInfo
    {
        IEnumerable<string> FilePathes {get;}
        CloudVerdict FlsCloud { get; }
        ValkyrieVerdict PreviousValkyrieVerdict { get; set; }
        long Size { get; }
        void AddFullPath(IComputer computer, string path);
        bool Equals(object obj);
        string GetFullPath();
        int GetHashCode();
        string GetMachineName();
        string GetNetworkPath(string path, string host = null);
        string GetNetworkPath(string hostName = null);
    }
}