using APTAT.Valkyrie.Client.Enums;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class BasicInfoResponse
    {
        public string first_analysis_date { get; set; }
        public string last_analysis_date { get; set; }
        public float malware_probability { get; set; }
        /// <summary>
        /// meaning file is analyzed/not analyzed by FVS before
        /// </summary>
        public bool Presence { get; set; }
        public int Return_code { get; set; }
        public string Sha1 { get; set; }
        public string Submit_token { get; set; }
        public VerdictEnum verdict { get; set; }
        private bool white_listed { get; set; }
    }
}
