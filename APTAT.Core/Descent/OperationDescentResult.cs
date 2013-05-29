using CRM.SrvCore.Descent;

namespace APTAT.Core.Descent
{
	public struct OperationDescentResult
	{
		public OperationDescentResult( DescentStage scope, bool completed, bool succeeded,
			FormattedText statusMessage, FormattedText shortStatusMessage,
			string errorCode, string systemMessage, int systemCode, int progressPercentage )
			: this( scope, completed, succeeded, statusMessage, shortStatusMessage, errorCode, systemMessage, systemCode, progressPercentage, null, null, null )
		{ }

		public OperationDescentResult( DescentStage scope, bool completed, bool succeeded, 
			FormattedText statusMessage, FormattedText shortStatusMessage,
			string errorCode, string systemMessage, int systemCode, int progressPercentage, string[] extensionTexts, string[] uninstalledSoftware, string outdatedCis )
			: this()
		{
			Stage = scope;
			Completed = completed;
			Succeeded = succeeded;
			StatusMessage = statusMessage;
			ShortStatusMessage = shortStatusMessage;
			ErrorCode = errorCode;
			SystemMessage = systemMessage;
			SystemCode = systemCode;
			ProgressPercentage = progressPercentage;
			ExtensionTexts = extensionTexts;
			UninstalledSoftware = uninstalledSoftware;
			OutdatedCis = outdatedCis;
		}

		public bool Completed { get; set; }
		public bool Succeeded { get; set; }
		public FormattedText StatusMessage { get; set; }
		public FormattedText ShortStatusMessage { get; set; }
		public string ErrorCode { get; set; }
		public string SystemMessage { get; set; }
		public int SystemCode { get; set; }
		public DescentStage Stage { get; set; }
		public int ProgressPercentage { get; set; }
		public string[] ExtensionTexts { get; set; }
		public string[] UninstalledSoftware { get; set; }
		public string OutdatedCis { get; set; }
	}
}