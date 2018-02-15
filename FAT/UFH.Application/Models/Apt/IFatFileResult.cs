using APTAT.Core.Valkyrie;

namespace UFH.Application.Models.Apt
{
    public interface IFatFileResult : IBadFileItem
    {
        ValkyrieVerdict? ValkyrieVerdictValue { get; set; }
        float? MalwareProbabilityValue { get; set; }
        string SkipReason { get; set; }
        string OperationStatus { get; set; }
        bool IsWhiteListed { get; set; }
    }
}