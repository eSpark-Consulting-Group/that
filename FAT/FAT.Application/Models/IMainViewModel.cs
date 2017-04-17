

namespace FAT.Application.Models.ModelBase
{
    public interface IMainViewModel
    {
        string Login { get; set; }
        string Password { get; set; }
        string CurrentDomainName { get; set; }
        WizardStepEnum CurrentStep { get; set; }
        bool ValidateCredentials(out string errorMessage);
    }
}
