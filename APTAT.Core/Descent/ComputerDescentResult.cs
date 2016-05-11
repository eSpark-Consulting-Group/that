using System;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
	public struct ComputerDescentResult
	{
		private void Initialize(IComputer computer )
		{


			Computer = computer;
			StatusMessage = FormattedText.Empty;
			ShortStatusMessage = FormattedText.Empty;
			ErrorCode = String.Empty;
			SystemMessage = String.Empty;
			SystemCode = 0;
			ExtensionTexts = null;
		}

		public ComputerDescentResult( Guid targetUid, IComputer computer)
			: this()
		{
			Initialize( computer );
			TargetUid = targetUid;
		}

		public ComputerDescentResult( ComputerDescentResult originalResult, OperationDescentResult result )
			: this()
		{
			TargetUid = originalResult.TargetUid;
			Computer = originalResult.Computer;

			StatusMessage = result.StatusMessage;
			ShortStatusMessage = result.ShortStatusMessage;
			ProgressPercentage = result.ProgressPercentage;
			ErrorCode = result.ErrorCode;
			SystemMessage = result.SystemMessage;
			SystemCode = result.SystemCode;
            Succeeded = result.Succeeded;
            AgentCompleted = true;
        }

        public ComputerDescentResult(ComputerDescentResult originalResult, FormattedText statusMessage, FormattedText shortStatusMessage, int progressPercentage)
			: this()
		{
			TargetUid = originalResult.TargetUid;
			Computer = originalResult.Computer;
			ErrorCode = originalResult.ErrorCode;
			SystemMessage = originalResult.SystemMessage;
			SystemCode = originalResult.SystemCode;
			AgentCompleted = originalResult.AgentCompleted;
            Succeeded = originalResult.Succeeded;

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
		}

		public void Invalidate(bool invalidateAgent, DescentStatus errorCode, string shortMessage, string detailedMessage)
		{
			if (invalidateAgent)
			{
                Succeeded = false;
				AgentCompleted = true;
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

		public bool Completed
		{
			get
			{
				return  AgentCompleted || (AgentCompleted && !Succeeded);
			}
		}

		public bool Succeeded
		{

            get;
            private set;
        }

		public bool AgentCompleted
		{
			get;
			private set;
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

				var totalStages = 1;
				var completedStages = AgentCompleted ? 1 : 0;


				var completedPercentage = (double) completedStages / totalStages * 100;
				var totalPercentage = completedPercentage + 1.0 / totalStages * ProgressPercentage;

				return (int) totalPercentage;
			}
		}
	}
}
