using System.Threading;

namespace APTAT.Core.Descent
{
	public abstract class DescentWatcherParams
	{
	}

	public class ComputerDescentWatcherParams : DescentWatcherParams
	{
		public IComputer Computer
		{
			get;
			set;
		}

		public IDescentQueryResult DescentResult
		{
			get;
			set;
		}
	}

	public class AgentDescentWatcherParams : DescentWatcherParams
	{

		public string Computer
		{
			get;
			set;
		}

		public Thread DescentThread
		{
			get;
			set;
		}

		public IDescentCallback DescentCallback
		{
			get;
			set;
		}

	}
}
