

namespace FAT.Application.Models.ModelBase
{
    public enum WizardStepEnum
    {
        Domain,
        Workgroup,
        NetworkAddresses,
        Local,
        AutoDiscovery,
        None
    }

    public interface IWizardStepViewModel
    {
        WizardStepEnum Step { get; }
    }
}
