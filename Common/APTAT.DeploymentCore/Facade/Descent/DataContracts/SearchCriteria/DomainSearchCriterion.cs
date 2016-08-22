namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria
{
	public class DomainSearchCriterion : TargetsSearchCriterion
	{
		public string ParentAddress { get; set; }
		public bool FromRoot { get; set; }
		public string Domain { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
	}
}