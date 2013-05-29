namespace APTAT.Core.Facade.Descent.DataContracts
{
	public class CompletedStep
	{
		public int Index { get; set; }

		public DeploymentStep Step { get; set; }

		public CompletionStatus Status { get; set; }
	}
}
