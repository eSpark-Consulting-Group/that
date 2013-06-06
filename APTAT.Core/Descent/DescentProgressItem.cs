using System;

namespace APTAT.Core.Descent
{
	public class DescentProgressItem : IDescentProgressItem, IBusinessObject
	{
        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage)
            : this(computerUid, hostName, operationUid,statusMessage, scanStage, 0, 0, 0, 0)
		{
		}

        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount)
            : this(computerUid, hostName, operationUid, statusMessage, scanStage, totalFilesCount, unknownFilesCount, 0, 0)
        {
        }

        public DescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount, int malwaresCount, int safeFilesCount)
        {
            ComputerUid = computerUid;
            HostName = hostName;
            OperationUid = operationUid;
            StatusMessage = statusMessage;
            //ProgressPercentage = progressPercentage;
            ScanStage = scanStage;
            TotalFilesCount = totalFilesCount;
            UnknownFilesCount = unknownFilesCount;
            MalwaresCount = malwaresCount;
            SafeFilesCount = safeFilesCount;
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

        public string HostName { get; private set; }
	    public AptScanStage ScanStage { get; private set; }
	    public int TotalFilesCount { get; private set; }
	    public int UnknownFilesCount { get; private set; }
	    public int MalwaresCount { get; private set; }
	    public int SafeFilesCount { get; private set; }

	    #endregion
	}
}
