using APTAT.DeploymentCore.Messages;
using System;

namespace APTAT.DeploymentCore.Descent
{
	public class OperationDescentResult : IOperationDescentResult
    {
		public OperationDescentResult(Guid computerUid, bool succeeded,
			FormattedText statusMessage, FormattedText shortStatusMessage,
			string errorCode, string systemMessage, int systemCode,
            bool isCanceled)
        {
            ComputerUid = computerUid;
            Succeeded = succeeded;
            StatusMessage = statusMessage;
            ShortStatusMessage = shortStatusMessage;
            ErrorCode = errorCode;
            SystemMessage = systemMessage;
            SystemCode = systemCode;
            IsCanceled = isCanceled;

        }

        public bool IsCanceled { get; }
		public bool Succeeded { get;  }
		public FormattedText StatusMessage { get;  }
		public FormattedText ShortStatusMessage { get;  }
		public string ErrorCode { get;  }
		public string SystemMessage { get;  }
		public int SystemCode { get;  }
        public Guid ComputerUid { get; }
    }
}