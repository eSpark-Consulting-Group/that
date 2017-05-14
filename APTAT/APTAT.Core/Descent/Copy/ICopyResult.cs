using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Copy
{
    public interface ICopyResult : IComputerDescentResult
    {
        string[] FilePathes { get; }
    }
}