using System;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
    public class ComputerDescentResult : IComputerDescentResultInternal
    {

        public Guid TargetUid { get; }
        public IComputer Computer { get; }
    	public ComputerDescentResult( IComputer computer)			
		{
            Computer = computer;
            StatusMessage = FormattedText.Empty;
            ShortStatusMessage = FormattedText.Empty;
            ErrorCode = String.Empty;
            SystemMessage = String.Empty;
            Modified = DateTime.UtcNow;
            SystemCode = 0;
            TargetUid = computer.Uid;
		}

		public void UpdateStatus(IOperationDescentResult result)			
		{
            Modified = DateTime.UtcNow;
			StatusMessage = result.StatusMessage;
			ShortStatusMessage = result.ShortStatusMessage;
			ErrorCode = result.ErrorCode;
			SystemMessage = result.SystemMessage;
			SystemCode = result.SystemCode;
            Succeeded = result.Succeeded;
		    Percentage = 100;
            Completed = true;
        }

        public void UpdateStatus(FormattedText statusMessage, double percentage)			
		{
			Percentage = percentage;
            Modified = DateTime.UtcNow;

            if (statusMessage != null && !String.IsNullOrEmpty(statusMessage.ToString()))
            {
                StatusMessage = statusMessage;
                ShortStatusMessage = statusMessage;
            }
		}


        public void Invalidate(DescentStatus errorCode, string shortMessage, string detailedMessage)
		{
			Succeeded = false;
            Completed = true;
            Percentage = 100;
            Modified = DateTime.UtcNow;
			ErrorCode = errorCode.ToString();
			ShortStatusMessage = new FormattedText(new ResourceText(shortMessage, shortMessage));
			StatusMessage = new FormattedText(new ResourceText(detailedMessage, detailedMessage));
			SystemMessage = detailedMessage;
		}

        public void SetSystemKind(SystemKind systemKind)
        {
            Computer.SystemKind = systemKind;
            Modified = DateTime.UtcNow;
        }

        public double Percentage { get; private set; }
        public DateTime Modified { get; private set; }
		public bool Completed { get; private set; }
		public bool Succeeded { get; private set; }
		public FormattedText StatusMessage { get; private set; }
		public FormattedText ShortStatusMessage { get; private set; }
	    public string ErrorCode { get; private set; }
	    public string SystemMessage { get; private set; }
		public int SystemCode { get; private set; }
	}
}
