using System.Collections.ObjectModel;

namespace APTAT.Valkyrie
{
    public interface IValkyrieFileInfo
    {
        CloudVerdict FlsCloud { get;  }
        string Ip { get;  }
        bool IsLocalComputer { get;  }
        string Path { get;  }
        ReadOnlyCollection<string> Paths { get; }
        ValkyrieVerdict PreviousValkyrieVerdict { get; set; }
        string Sha1 { get; }
        long Size { get;  }

        void AddFullPath(string ip, string path);
        bool Equals(object obj);
        string GetFullPath();
        int GetHashCode();
        string GetMachineName();
        string GetNetworkPath();
    }
}