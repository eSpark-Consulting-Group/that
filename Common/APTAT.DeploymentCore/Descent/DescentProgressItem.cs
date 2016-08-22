using System;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
	public class DescentProgressItem : IDescentProgressItem
	{
        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, double percentage)
            : this (computerUid, hostName, operationUid, statusMessage, percentage, false, false)
        {
        }

        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, double percentage,
            bool isSuccess, bool isFinish)
        {
            ComputerUid = computerUid;
            HostName = hostName;
            OperationUid = operationUid;
            StatusMessage = statusMessage;
            ProgressPercentage = percentage;
            IsSuccess = isSuccess;
            IsFinish = isFinish;
        }
		

        #region IDescentProgressItem Members

        public Guid ComputerUid { get; private set; }
		public Guid OperationUid { get; private set; }
		public double ProgressPercentage { get; private set; }
		public FormattedText StatusMessage { get; private set; }
        public string HostName { get; private set; }

        public virtual bool IsSuccess { get; private set; }
        public virtual bool IsFinish { get; private set; }
        public virtual bool IsCanceled { get; set; }
	    #endregion
	}
}
