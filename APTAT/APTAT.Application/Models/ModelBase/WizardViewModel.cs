using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
    public class ExpandCommandContext
    {
        public Object DirectoryItem { get; set; }

        public Action<ExpandCommandContext> Completed { get; set; }

        public Object TreeItem { get; set; }

        public void RaiseCompleted()
        {
            if (Completed != null)
            {
                Completed(this);
            }
        }
    }

    public class ChangeStepCommandContext
    {
        public Int32 Step { get; set; }

        public Action<ChangeStepCommandContext> Completed { get; set; }

        public Boolean Cancel { get; set; }

        public Boolean IsNext { get; set; }

        public void RaiseCompleted()
        {
            if (Completed != null)
            {
                Completed(this);
            }
        }

        public Boolean IsForward(Int32 currentStep)
        {
            return Step > currentStep;
        }

        public Boolean IsBackward(Int32 currentStep)
        {
            return currentStep > Step;
        }
    }


    public class WizardViewModel : ViewModel
    {
        public event EventHandler Completed;
        public event EventHandler Cancelled;

        public DelegateCommand ChangeStepCommand { get; private set; }
        protected String[] _steps;
        private Boolean _isNextEnabled;
        private Boolean _isBackEnabled;
        private Boolean _isFinishEnabled;
        private Boolean _isCancelEnabled;
        private Int32 _currentStepIndex;
        private Int32 _entryIndex;
        private String _currentStep;

        public object ParentViewModel
        {
            get { return App.Current.MainWindow.DataContext; }
        }

        public Int32 StepsCount
        {
            get { return _steps.Length; }
        }

        public String CurrentStep
        {
            get { return _currentStep; }
            private set
            {
                _currentStep = value;
                RaisePropertyChanged("CurrentStep");
            }
        }

        public Int32 CurrentStepIndex
        {
            get { return _currentStepIndex; }
            private set
            {
                _currentStepIndex = value;
                RaisePropertyChanged("CurrentStepIndex");
            }
        }

        public Boolean IsNextEnabled
        {
            get { return _isNextEnabled; }
            protected set
            {
                _isNextEnabled = value;
                RaisePropertyChanged("IsNextEnabled");
            }
        }

        public Boolean IsBackEnabled
        {
            get { return _isBackEnabled; }
            protected set
            {
                _isBackEnabled = value;
                RaisePropertyChanged("IsBackEnabled");
            }
        }

        public Boolean IsFinishEnabled
        {
            get { return _isFinishEnabled; }
            protected set
            {
                _isFinishEnabled = value;
                RaisePropertyChanged("IsFinishEnabled");
            }
        }

        public Boolean IsCancelEnabled
        {
            get { return _isCancelEnabled; }
            protected set
            {
                _isCancelEnabled = value;
                RaisePropertyChanged("IsCancelEnabled");
            }
        }

        public virtual string NextName
        {
            get { return "Next"; }
        }

        public void SetSteps(String[] steps)
        {
            _steps = steps;
        }

        public WizardViewModel()
        {
            ChangeStepCommand = new DelegateCommand(InvokeChangeStep);
            IsCancelEnabled = true;
        }

        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

            CurrentStepIndex = -1;
            do
            {
                CurrentStepIndex++;
                CurrentStep = _steps[CurrentStepIndex];
            }
            while (CurrentStep == null);
            _entryIndex = CurrentStepIndex;
            UpdateState();
        }

        private void InvokeChangeStep(Object arg)
        {
            ChangeStepCommandContext commandContext = (ChangeStepCommandContext)arg;
            OnStepChanging(() =>
            {
                if (commandContext.Step >= StepsCount)
                {
                    OnCompleted();
                    if (Completed != null)
                    {
                        Completed(this, EventArgs.Empty);
                    }
                }
                else if (commandContext.Step < _entryIndex)
                {
                    OnCancelled();
                    if (Cancelled != null)
                    {
                        Cancelled(this, EventArgs.Empty);
                    }
                }
                else
                {
                    CurrentStepIndex = commandContext.Step;
                    CurrentStep = _steps[CurrentStepIndex];

                    UpdateState();

                    OnStepChanged();
                    commandContext.RaiseCompleted();
                }
            }, CurrentStep, commandContext);
        }

        protected virtual void UpdateState()
        {
            if (CurrentStepIndex == 0 && CurrentStepIndex == _steps.Length - 1)
            {
                IsBackEnabled = false;
                IsNextEnabled = false;
                IsFinishEnabled = true;
            }
            else if (CurrentStepIndex == 0)
            {
                IsBackEnabled = false;
                IsNextEnabled = true;
                IsFinishEnabled = false;
            }
            else if (CurrentStepIndex == _steps.Length - 1)
            {
                IsBackEnabled = true;
                IsNextEnabled = false;
                IsFinishEnabled = true;
            }
            else
            {
                IsBackEnabled = true;
                IsNextEnabled = true;
                IsFinishEnabled = false;
            }
        }

        protected virtual void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            success();
        }

        protected virtual void OnStepChanged()
        {
        }


        protected virtual void OnCompleted()
        {
        }

        protected virtual void OnCancelled()
        {
        }
    }
}
