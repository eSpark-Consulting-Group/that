using APTAT.Core.Descent.Apt;
using APTAT.Valkyrie;

namespace APTAT.Core
{
    public interface IFinalVerdictCalculator
    {
        FinalVerdict GetFinalVerdict(Descent.Apt.CloudVerdict cloud, ValkyrieVerdict valkyrie);
        string GetVerdictString(FinalVerdict FinalVerdict, string operationStatus = null);
    }
}