using System;

namespace APTAT.Core.Facade.Descent.DataContracts
{
	public class DeploymentResult
	{
		public Guid TargetUid { get; set; }

		public string ComputerName { get; set; }

		public string IPAddress { get; set; }

		public DeploymentStep Step { get; set; }

		public bool Completed { get; set; }

		public bool Succeeded { get; set; }

		public int Progress { get; set; }

		public string ErrorCode { get; set; }

		public int SystemCode { get; set; }

		public string SystemMessage { get; set; }

		public string StatusMessage { get; set; }

		public string[] ExtensionTexts { get; set; }

		public CompletedStepsCollection CompletedSteps { get; set; }

		public int TotalProgress { get; set; }
	}
}
