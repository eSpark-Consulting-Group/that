using System;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
	public struct TargetUidComputerPair
	{
		public TargetUidComputerPair( Guid targetUid, IComputer computer )
			: this()
		{
			TargetUid = targetUid;
			Computer = computer;
		}

		public Guid TargetUid { get; set; }
		public IComputer Computer { get; set; }
		public object State { get; set; }
		public DescentStatus DescentStatus { get; set; }
	}
}
