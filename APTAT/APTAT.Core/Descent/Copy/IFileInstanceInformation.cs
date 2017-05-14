using APTAT.DeploymentCore;

namespace APTAT.Core.Descent.Copy
{
    public interface IFileInstanceInformation
    {
        IComputer Computer { get; }
        string Path { get; }
    }
}
