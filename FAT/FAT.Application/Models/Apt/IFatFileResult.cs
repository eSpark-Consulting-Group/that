using APTAT.Valkyrie;

namespace FAT.Application.Models.Apt
{
    public interface IFatFileResult : IBadFileItem
    {
        ValkyrieVerdict? ValkyrieVerdictValue { get; set; }
        float? MalwareProbabilityValue { get; set; }
        UiFinalVerdict FinalVerdict { get; }
        string SkipReason { get; set; }
        string OperationStatus { get; set; }
        bool IsWhiteListed { get; set; }
    }
}