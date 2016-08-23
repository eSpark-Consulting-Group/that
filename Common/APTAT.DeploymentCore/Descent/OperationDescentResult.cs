using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
	public struct OperationDescentResult : IOperationDescentResult
    {
		public OperationDescentResult( bool succeeded,
			FormattedText statusMessage, FormattedText shortStatusMessage,
			string errorCode, string systemMessage, int systemCode )
        {
            Succeeded = succeeded;
            StatusMessage = statusMessage;
            ShortStatusMessage = shortStatusMessage;
            ErrorCode = errorCode;
            SystemMessage = systemMessage;
            SystemCode = systemCode;
        }
        
        
		public bool Succeeded { get;  }
		public FormattedText StatusMessage { get;  }
		public FormattedText ShortStatusMessage { get;  }
		public string ErrorCode { get;  }
		public string SystemMessage { get;  }
		public int SystemCode { get;  }
	}
}