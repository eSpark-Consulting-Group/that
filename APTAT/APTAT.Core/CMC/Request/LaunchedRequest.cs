namespace APTAT.Core.CMC.Request
{
    public class LaunchedRequest : ILaunchedRequest
    {
        public string AcId { get; set; }
        public string ProductVersion { get; set; }
        public string ProductIdentifier { get; set; }
    }
}


