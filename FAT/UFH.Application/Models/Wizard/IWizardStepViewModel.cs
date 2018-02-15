

namespace UFH.Application.Models.ModelBase
{
    public enum WizardStepEnum
    {
        Domain,
        Workgroup,
        NetworkAddresses,
        Local,
        None
    }

    public interface IWizardStepViewModel
    {
        WizardStepEnum Step { get; }
    }
}
