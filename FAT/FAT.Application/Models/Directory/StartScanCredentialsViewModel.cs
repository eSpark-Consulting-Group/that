using APTAT.DeploymentCore;
using System;
using System.Collections.Generic;

namespace FAT.Application.Models.Directory
{
    public class StartScanCredentialsViewModel : WizardViewModel
    {
        public StartScanCredentialsViewModel(IEnumerable<IComputer> computers)
        {     
            UseCurrentDomainForDeploy = HasCurrentDomainName;
            RaisePropertyChanged(nameof(UseCurrentCredentials));

            CheckIfCanUseCurrentCredentials(computers);
        }

        protected override void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            var parent = ParentViewModel;
            if (parent != null)
            {
                string errorMessage;

                if (!UseCurrentDomainForDeploy && !parent.ValidateCredentials(out errorMessage))
                    Error = errorMessage;
                else
                {
                    parent.Login = Login;
                    parent.Password = Password;
                }
            }
        }

        private void CheckIfCanUseCurrentCredentials(IEnumerable<IComputer> computers)
        {
            UseCurrentCredentials = IsOnlyLocalComputer(computers);
        }

        private bool IsOnlyLocalComputer(IEnumerable<IComputer> computers)
        {
            foreach (var address in computers)
            {
                if (!address.IsLocalComputer)
                    return false;
            }

            return true;
        }
    }
}
