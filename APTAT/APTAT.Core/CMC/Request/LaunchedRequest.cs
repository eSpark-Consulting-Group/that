namespace APTAT.Core.CMC.Request
{
    public class LaunchedRequest : ILaunchedRequest
    {
        public string Acid { get; }
        public string ProductVersion { get; }
        public string ProductIdentifier { get; }
    }
}


