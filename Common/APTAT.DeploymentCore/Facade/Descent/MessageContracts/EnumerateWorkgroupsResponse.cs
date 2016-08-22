namespace APTAT.DeploymentCore.Facade.Descent.MessageContracts
{
	public class EnumerateWorkgroupsResponse
	{
		public bool IsCompleted { get; set; }

		public string[] Workgroups { get; set; }
	}
}
