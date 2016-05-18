using System;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
	public struct ComputerDescentResult
	{
		public ComputerDescentResult( Guid targetUid, IComputer computer)
			: this()
		{
            Computer = computer;
            StatusMessage = FormattedText.Empty;
            ShortStatusMessage = FormattedText.Empty;
            ErrorCode = String.Empty;
            SystemMessage = String.Empty;
            SystemCode = 0;
            ExtensionTexts = null;
            TargetUid = targetUid;
		}

		public ComputerDescentResult( ComputerDescentResult originalResult, OperationDescentResult result )
			: this()
		{
			TargetUid = originalResult.TargetUid;
			Computer = originalResult.Computer;

			StatusMessage = result.StatusMessage;
			ShortStatusMessage = result.ShortStatusMessage;
			ErrorCode = result.ErrorCode;
			SystemMessage = result.SystemMessage;
			SystemCode = result.SystemCode;
            Succeeded = result.Succeeded;
            Completed = true;
        }

        public ComputerDescentResult(ComputerDescentResult originalResult, FormattedText statusMessage, 
            FormattedText shortStatusMessage)
			: this()
		{
			TargetUid = originalResult.TargetUid;
			Computer = originalResult.Computer;
			ErrorCode = originalResult.ErrorCode;
			SystemMessage = originalResult.SystemMessage;
			SystemCode = originalResult.SystemCode;
			Completed = originalResult.Completed;
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
            
			ExtensionTexts = originalResult.ExtensionTexts;
		}

		public void Invalidate(bool invalidateAgent, DescentStatus errorCode, string shortMessage, string detailedMessage)
		{
			if (invalidateAgent)
			{
                Succeeded = false;
				Completed = true;
			}
			ErrorCode = errorCode.ToString();
			ShortStatusMessage = new FormattedText(new ResourceText(shortMessage, shortMessage));
			StatusMessage = new FormattedText(new ResourceText(detailedMessage, detailedMessage));
			SystemMessage = detailedMessage;
		}

		public Guid TargetUid { get; private set; }
		public IComputer Computer { get; private set; }
		public bool Completed { get; private set; }
		public bool Succeeded { get; private set; }
		public FormattedText StatusMessage { get; private set; }
		public FormattedText ShortStatusMessage { get; private set; }
	    public string ErrorCode { get; private set; }
	    public string SystemMessage { get; private set; }
		public int SystemCode { get; private set; }
		public string[] ExtensionTexts { get; private set; }
	}
}
