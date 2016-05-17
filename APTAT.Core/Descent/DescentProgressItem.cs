using System;

namespace APTAT.Core.Descent
{
	public class DescentProgressItem : IDescentProgressItem, IBusinessObject
	{
        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, int percentage)
            : this(computerUid, hostName, operationUid, statusMessage, scanStage, 0, 0, 0, 0, percentage, 0, 0)
		{
		}

        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount, int percentage, int errorFilesCount, int failedAtNodeCount)
            : this(computerUid, hostName, operationUid, statusMessage, scanStage, totalFilesCount, unknownFilesCount, 0, 0, percentage, errorFilesCount, failedAtNodeCount)
        {
        }

        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount, int malwaresCount, int safeFilesCount, int percentage, int errorFileCount, int failedAtNodeCount)
        {
            ComputerUid = computerUid;
            HostName = hostName;
            OperationUid = operationUid;
            StatusMessage = statusMessage;
            ProgressPercentage = percentage;
            ScanStage = scanStage;
            TotalFilesCount = totalFilesCount;
            UnknownFilesCount = unknownFilesCount;
            MalwaresCount = malwaresCount;
            SafeFilesCount = safeFilesCount;
            ErrorFilesCount = errorFileCount;
            FailedAtNodeCount = failedAtNodeCount;
        }

	    

	    #region IBusinessObject Members

		public long Key
		{
			get { return 0; }
		}

		public bool IsNew
		{
			get { return false; }
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			return this;
		}

		#endregion

		#region IDescentProgressItem Members

		public Guid ComputerUid
		{
			get;
			private set;
		}

		public Guid OperationUid
		{
			get;
			private set;
		}

		public int ProgressPercentage
		{
			get;
			private set;
		}

		public FormattedText StatusMessage
		{
			get;
			private set;
		}
        public int FailedAtNodeCount { get; set; }
        public string HostName { get; private set; }
	    public AptScanStage ScanStage { get; private set; }
	    public int TotalFilesCount { get; private set; }
	    public int UnknownFilesCount { get; private set; }
	    public int MalwaresCount { get; private set; }
	    public int SafeFilesCount { get; private set; }
        public int ErrorFilesCount { get; private set; }
	    #endregion
	}
}
