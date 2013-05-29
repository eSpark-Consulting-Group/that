namespace APTAT.Core.Facade.Descent.MessageContracts
{
	public class GetDescentErrorMessageRequest
	{
		public string ErrorCode { get; set; }

		public int SystemCode { get; set; }

		public string SystemMessage { get; set; }

		public string[] ExtensionTexts { get; set; }

        public string CultureInfoName { get; set; }
	}
}
