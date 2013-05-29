using System;

namespace APTAT.Core.Descent
{
	public class DescentProgressItem : IDescentProgressItem, IBusinessObject
	{
		public DescentProgressItem( Guid computerUid, Guid operationUid, FormattedText statusMessage, int progressPercentage )
		{
			ComputerUid = computerUid;
			OperationUid = operationUid;
			StatusMessage = statusMessage;
			ProgressPercentage = progressPercentage;
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

		#endregion
	}
}
