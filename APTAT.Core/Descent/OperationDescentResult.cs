

namespace APTAT.Core.Descent
{
	public struct OperationDescentResult
	{
		public OperationDescentResult(  bool completed, bool succeeded,
			FormattedText statusMessage, FormattedText shortStatusMessage,
			string errorCode, string systemMessage, int systemCode, int progressPercentage )
        {
            Succeeded = succeeded;
            StatusMessage = statusMessage;
            ShortStatusMessage = shortStatusMessage;
            ErrorCode = errorCode;
            SystemMessage = systemMessage;
            SystemCode = systemCode;
            ProgressPercentage = progressPercentage;
        }
        
        
		public bool Succeeded { get; set; }
		public FormattedText StatusMessage { get; set; }
		public FormattedText ShortStatusMessage { get; set; }
		public string ErrorCode { get; set; }
		public string SystemMessage { get; set; }
		public int SystemCode { get; set; }
		public int ProgressPercentage { get; set; }
	}
}