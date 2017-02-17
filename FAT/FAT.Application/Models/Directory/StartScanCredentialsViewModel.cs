using APTAT.DeploymentCore;
using System;
using System.Collections.Generic;
using Composition = APTAT.Common.CompositionHelper;

namespace FAT.Application.Models.Directory
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class StartScanCredentialsViewModel : WizardViewModel
    {
        public StartScanCredentialsViewModel()
        {               
        }

        private bool _useCurrentNameDetermined;

        protected override void InvokeViewLoaded(object arg)
        {
            if (!_useCurrentNameDetermined)
            {
                UseCurrentDomainForDeploy = HasCurrentDomainName;
                RaisePropertyChanged(nameof(UseCurrentCredentials));

                _useCurrentNameDetermined = true;
            }

            Password = null;
        }

        protected override void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            var parent = ParentViewModel;
            if (parent != null)
            {
                parent.Login = Login;
                parent.Password = Password;

                Error = null;
                string errorMessage;

                if (!UseCurrentDomainForDeploy && !parent.ValidateCredentials(out errorMessage))
                    Error = errorMessage;
            }
        }

        public void CheckIfCanUseCurrentCredentials(IEnumerable<IComputer> computers)
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
