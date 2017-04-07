using System;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
    public class ComputerDescentResult : IComputerDescentResultInternal
    {
        protected readonly ITimeStampProvider TimeStampProvider;
        public Guid TargetUid { get; }
        public IComputer Computer { get; }
        public Guid OperationUid { get; }
        public double Percentage { get; private set; }
        public TimeSpan Modified { get; protected set; }
        public bool Completed { get; private set; }
        public bool Succeeded { get; private set; }
        public FormattedText StatusMessage { get; private set; }
        public FormattedText ShortStatusMessage { get; private set; }
        public string ErrorCode { get; private set; }
        public bool IsCanceled { get; private set; }
        public string SystemMessage { get; private set; }
        public int SystemCode { get; private set; }
        public ComputerDescentResult(IComputer computer, Guid operationUid, ITimeStampProvider timeStampProvider)
        {
            Computer = computer;
            StatusMessage = FormattedText.Empty;
            ShortStatusMessage = FormattedText.Empty;
            ErrorCode = String.Empty;
            OperationUid = operationUid;
            SystemMessage = String.Empty;
            SystemCode = 0;
            TargetUid = computer.Uid;
            TimeStampProvider = timeStampProvider;
            Modified = TimeStampProvider?.GetCurretTimestamp()??TimeSpan.Zero;
        }

		public virtual void UpdateStatus(IOperationDescentResult result)			
		{
			StatusMessage = result.StatusMessage;
			ShortStatusMessage = result.ShortStatusMessage;
			ErrorCode = result.ErrorCode;
			SystemMessage = result.SystemMessage;
			SystemCode = result.SystemCode;
            Succeeded = result.Succeeded;
		    IsCanceled = result.IsCanceled;
            Percentage = 100;
            Completed = true;
            Modified = TimeStampProvider.GetCurretTimestamp();
        }

        public virtual void UpdateStatus(IDescentProgressItem progress)
        {
            var percentage = progress.ProgressPercentage;
            var statusMessage = progress.StatusMessage;
            IsCanceled = progress.IsCanceled;
            var update = false;
            if (Math.Abs(percentage - Percentage) > 0.1)
            {
                Percentage = percentage;
                update = true;
            }

            if (statusMessage != null && !String.IsNullOrEmpty(statusMessage.ToString())
                && !Equals(statusMessage, ShortStatusMessage))
            {
                StatusMessage = statusMessage;
                ShortStatusMessage = statusMessage;
                update = true;
            }
            if (update)
            {
                Completed = false;
                Modified = TimeStampProvider.GetCurretTimestamp();
            }
		}

        public void Invalidate(DescentStatus errorCode, string shortMessage, string detailedMessage)
		{
			Succeeded = false;
            Completed = true;
            Percentage = 100;
			ErrorCode = errorCode.ToString();
			ShortStatusMessage = new FormattedText(new ResourceText(shortMessage, shortMessage));
			StatusMessage = new FormattedText(new ResourceText(detailedMessage, detailedMessage));
			SystemMessage = detailedMessage;
            Modified = TimeStampProvider.GetCurretTimestamp();
        }
        public void SetSystemKind(SystemKind systemKind)
        {
            Computer.SystemKind = systemKind;
            Modified = TimeStampProvider.GetCurretTimestamp();
        }
    }
}
