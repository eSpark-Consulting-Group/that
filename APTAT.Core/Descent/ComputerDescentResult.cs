using System;
using CRM.SrvCore.Descent;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
	public struct ComputerDescentResult
	{
		private bool _packageCompleted;
		private bool _packageSucceeded;
		private bool _uninstallCompleted;
		private bool _uninstallSucceeded;
		private readonly bool _outdatedCisUninstallCompleted;
		private readonly bool _outdatedCisUninstallSucceeded;

		private void Initialize( bool withUninstall, bool withPackage, IComputer computer )
		{
			if( withUninstall && !withPackage )
			{
				throw new ArgumentException( "Cannot start descent operation with incompatible products uninstall but without package install." );
			}

			Computer = computer;
			WithPackage = withPackage;
			WithUninstall = withUninstall;
			StatusMessage = FormattedText.Empty;
			ShortStatusMessage = FormattedText.Empty;
			ErrorCode = String.Empty;
			SystemMessage = String.Empty;
			SystemCode = 0;
			ExtensionTexts = null;
			UninstalledSoftware = null;
			OutdatedCis = null;
		}

		public ComputerDescentResult( IComputer computer, bool withPackage, bool withUninstall )
			: this()
		{
			Initialize( withUninstall, withPackage, computer );
			TargetUid = Guid.NewGuid();
		}

		public ComputerDescentResult( Guid targetUid, IComputer computer, bool withPackage, bool withUninstall )
			: this()
		{
			Initialize( withUninstall, withPackage, computer );
			TargetUid = targetUid;
		}

		public ComputerDescentResult( ComputerDescentResult originalResult, OperationDescentResult result )
			: this()
		{
			TargetUid = originalResult.TargetUid;
			Computer = originalResult.Computer;
			WithPackage = originalResult.WithPackage;
			WithUninstall = originalResult.WithUninstall;

			StatusMessage = result.StatusMessage;
			ShortStatusMessage = result.ShortStatusMessage;
			ProgressPercentage = result.ProgressPercentage;
			ErrorCode = result.ErrorCode;
			SystemMessage = result.SystemMessage;
			SystemCode = result.SystemCode;
			ExtensionTexts = result.ExtensionTexts;
			UninstalledSoftware = result.UninstalledSoftware ?? originalResult.UninstalledSoftware;
			OutdatedCis = result.OutdatedCis ?? originalResult.OutdatedCis;
			switch( result.Stage )
			{
				case DescentStage.AgentInstall:
					AgentCompleted = result.Completed;
					AgentSucceeded = result.Succeeded;
					_packageCompleted = originalResult.PackageCompleted;
					_packageSucceeded = originalResult.PackageSucceeded;
					_uninstallCompleted = originalResult.UninstallCompleted;
					_uninstallSucceeded = originalResult.UninstallSucceeded;
					_outdatedCisUninstallCompleted = originalResult.OutdatedCisUninstallCompleted;
					_outdatedCisUninstallSucceeded = originalResult.OutdatedCisUninstallSucceeded;
					break;
				case DescentStage.PackageInstall:
					AgentCompleted = originalResult.AgentCompleted;
					AgentSucceeded = originalResult.AgentSucceeded;
					_packageCompleted = result.Completed;
					_packageSucceeded = result.Succeeded;
					_uninstallCompleted = originalResult.UninstallCompleted;
					_uninstallSucceeded = originalResult.UninstallSucceeded;
					_outdatedCisUninstallCompleted = originalResult.OutdatedCisUninstallCompleted;
					_outdatedCisUninstallSucceeded = originalResult.OutdatedCisUninstallSucceeded;
					break;
				case DescentStage.IncompatibleUninstall:
					AgentCompleted = originalResult.AgentCompleted;
					AgentSucceeded = originalResult.AgentSucceeded;
					_packageCompleted = originalResult.Completed;
					_packageSucceeded = originalResult.Succeeded;
					_uninstallCompleted = result.Completed;
					_uninstallSucceeded = result.Succeeded;
					_outdatedCisUninstallCompleted = originalResult.OutdatedCisUninstallCompleted;
					_outdatedCisUninstallSucceeded = originalResult.OutdatedCisUninstallSucceeded;
					break;
				case DescentStage.OutdatedCisUninstall:
					AgentCompleted = originalResult.AgentCompleted;
					AgentSucceeded = originalResult.AgentSucceeded;
					_packageCompleted = originalResult.Completed;
					_packageSucceeded = originalResult.Succeeded;
					_uninstallCompleted = originalResult.UninstallCompleted;
					_uninstallSucceeded = originalResult.UninstallSucceeded;
					_outdatedCisUninstallCompleted = result.Completed;
					_outdatedCisUninstallSucceeded = result.Succeeded;
					break;
			}
		}

		public ComputerDescentResult(ComputerDescentResult originalResult, FormattedText statusMessage, FormattedText shortStatusMessage, int progressPercentage)
			: this()
		{
			TargetUid = originalResult.TargetUid;
			Computer = originalResult.Computer;
			WithPackage = originalResult.WithPackage;
			WithUninstall = originalResult.WithUninstall;
			ErrorCode = originalResult.ErrorCode;
			SystemMessage = originalResult.SystemMessage;
			SystemCode = originalResult.SystemCode;
			AgentCompleted = originalResult.AgentCompleted;
			AgentSucceeded = originalResult.AgentSucceeded;
			_packageCompleted = originalResult.PackageCompleted;
			_packageSucceeded = originalResult.PackageSucceeded;
			_uninstallCompleted = originalResult.UninstallCompleted;
			_uninstallSucceeded = originalResult.UninstallSucceeded;
			_outdatedCisUninstallCompleted = originalResult.OutdatedCisUninstallCompleted;
			_outdatedCisUninstallSucceeded = originalResult.OutdatedCisUninstallSucceeded;

			if( statusMessage == null || String.IsNullOrEmpty( statusMessage.ToString() ) )
			{
				StatusMessage = originalResult.StatusMessage;
			}
			else
			{
				StatusMessage = statusMessage;
			}

			if( shortStatusMessage == null || String.IsNullOrEmpty( shortStatusMessage.ToString() ) )
			{
				ShortStatusMessage = originalResult.ShortStatusMessage;
			}
			else
			{
				ShortStatusMessage = shortStatusMessage;
			}

			ProgressPercentage = progressPercentage;
			ExtensionTexts = originalResult.ExtensionTexts;
			UninstalledSoftware = originalResult.UninstalledSoftware;
			OutdatedCis = originalResult.OutdatedCis;
		}

		public void Invalidate(bool invalidateAgent, bool invalidateUninstall, bool invalidatePackage, DescentStatus errorCode, string shortMessage, string detailedMessage)
		{
			if (invalidateAgent)
			{
				AgentSucceeded = false;
				AgentCompleted = true;
			}
			if (invalidateUninstall && WithUninstall)
			{
				_uninstallSucceeded = false;
				_uninstallCompleted = true;
			}
			if (invalidatePackage && WithPackage)
			{
				_packageSucceeded = false;
				_packageCompleted = true;
			}
			ErrorCode = errorCode.ToString();
			ShortStatusMessage = new FormattedText(new ResourceText(shortMessage, shortMessage));
			StatusMessage = new FormattedText(new ResourceText(detailedMessage, detailedMessage));
			SystemMessage = detailedMessage;
		}

		public Guid TargetUid
		{
			get;
			private set;
		}

		public IComputer Computer
		{
			get;
			private set;
		}

		public bool WithPackage
		{
			get;
			private set;
		}

		public bool WithUninstall
		{
			get;
			private set;
		}

		public bool Failed
		{
			get
			{
				return
					( AgentCompleted && !AgentSucceeded ) ||
					( OutdatedCisUninstallCompleted && !OutdatedCisUninstallSucceeded ) ||
					( UninstallCompleted && !UninstallSucceeded ) ||
					( PackageCompleted && !PackageSucceeded );
			}
		}

		public bool Completed
		{
			get
			{
				return ( AgentCompleted && OutdatedCisUninstallCompleted && UninstallCompleted && PackageCompleted ) || Failed;
			}
		}

		public bool Succeeded
		{
			get
			{
				return AgentSucceeded && OutdatedCisUninstallSucceeded && PackageSucceeded && UninstallSucceeded;
			}
		}

		public bool AgentCompleted
		{
			get;
			private set;
		}

		public bool AgentSucceeded
		{
			get;
			private set;
		}

		public bool PackageCompleted
		{
			get
			{
				return _packageCompleted || !WithPackage;
			}
		}

		public bool PackageSucceeded
		{
			get
			{
				return _packageSucceeded || !WithPackage;
			}
		}

		public bool UninstallCompleted
		{
			get
			{
				return _uninstallCompleted || !WithUninstall;
			}
		}

		public bool UninstallSucceeded
		{
			get
			{
				return _uninstallSucceeded || !WithUninstall;
			}
		}

		public bool OutdatedCisUninstallCompleted
		{
			get
			{
				return _outdatedCisUninstallCompleted || !WithPackage;
			}
		}

		public bool OutdatedCisUninstallSucceeded
		{
			get
			{
				return _outdatedCisUninstallSucceeded || !WithPackage;
			}
		}

		public FormattedText StatusMessage
		{
			get;
			private set;
		}

		public FormattedText ShortStatusMessage
		{
			get;
			private set;
		}

		public string ErrorCode
		{
			get;
			private set;
		}

		public string SystemMessage
		{
			get;
			private set;
		}

		public int SystemCode
		{
			get;
			private set;
		}

		public int ProgressPercentage
		{
			get; 
			private set; 
		}

		public string[] ExtensionTexts
		{
			get;
			private set;
		}

		public string[] UninstalledSoftware
		{
			get;
			private set;
		}

		public string OutdatedCis
		{
			get;
			private set;
		}

		public DescentStage CurrentStage
		{
			get
			{
				if( !AgentCompleted )
					return DescentStage.AgentInstall;
				if( !AgentSucceeded )
					return DescentStage.AgentInstall;
				if( !WithPackage )
					return DescentStage.AgentInstall;
				if( WithUninstall )
				{
					if( !UninstallCompleted )
						return DescentStage.IncompatibleUninstall;
					if( !UninstallSucceeded )
						return DescentStage.IncompatibleUninstall;
				}

				if( !OutdatedCisUninstallCompleted )
					return DescentStage.OutdatedCisUninstall;
				if( !OutdatedCisUninstallSucceeded )
					return DescentStage.OutdatedCisUninstall;

				return DescentStage.PackageInstall;
			}
		}

		public int TotalProgressPercentage
		{
			get
			{
				if (Completed)
					return 100;

				// As we can't know beforehand whether we need to uninstall outdated CIS,
				// we assume we always have to, and just momentarily increase the total
				// percentage in case no outdated CIS exists (hence WithPackage counts for
				// two stages).

				var totalStages = 1 + (WithUninstall ? 1 : 0) + (WithPackage ? 2 : 0);
				var completedStages = 0;
				switch (CurrentStage)
				{
					case DescentStage.AgentInstall:
						completedStages = 0;
						break;
					case DescentStage.IncompatibleUninstall:
						completedStages = 1;
						break;
					case DescentStage.OutdatedCisUninstall:
						completedStages = WithUninstall ? 2 : 1;
						break;
					case DescentStage.PackageInstall:
						completedStages = WithUninstall ? 3 : 2;
						break;
				}

				var completedPercentage = (double) completedStages / totalStages * 100;
				var totalPercentage = completedPercentage + 1.0 / totalStages * ProgressPercentage;

				return (int) totalPercentage;
			}
		}
	}
}
