using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;

namespace APTAT.Core
{
    public interface IFinalVerdictCalculator
    {
        FinalVerdict GetFinalVerdict(Descent.Apt.CloudVerdict cloud, ValkyrieVerdict valkyrie);
        string GetVerdictString(FinalVerdict finalVerdict, string operationStatus = null);
    }
}