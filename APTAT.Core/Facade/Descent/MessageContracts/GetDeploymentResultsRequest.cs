using System;

namespace APTAT.Core.Facade.Descent.MessageContracts
{
	public class GetDeploymentResultsRequest
	{
		public Guid OperationUid { get; set; }

		public int StartFrom { get; set; }

		public int Count { get; set; }
	}
}
