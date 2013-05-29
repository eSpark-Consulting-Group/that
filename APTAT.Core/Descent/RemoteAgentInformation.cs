using System;

namespace APTAT.Core.Descent
{
	[Serializable]
	public class RemoteAgentInformation
	{
		private bool _installed;
		private bool _outdated;
		private bool _currentServerAccessible;
	    private string _errorDetails;

		public string ErrorDetails
		{
			get { return _errorDetails; }
			internal set { _errorDetails = value; }
		}

		#region Properties

		public IComputer Computer { get; set; }

		public string ServerHostName { get; set; }

		public uint ServerPort { get; set; }

		public uint VersionMajor { get; set; }

		public uint VersionMinor { get; set; }

		public bool Installed
		{
			get { return _installed; }
			internal set { _installed = value; }
		}

		public bool Outdated
		{
			get { return _outdated; }
			internal set { _outdated = value; }
		}

		public bool CurrentServerAccessible
		{
			get { return _currentServerAccessible; }
			internal set { _currentServerAccessible = value; }
		}

	    #endregion
	}
}
