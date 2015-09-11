using APTAT.Valkyrie.Client.Enums;
using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class StartAndFinalizeScanResponse
    {
        [JsonProperty("scan_id ")]
        public string ScanId { get; set; } //a key uniquely identifying this scan

        [JsonProperty("return_code")]
        public ErrorCodesEnum ReturnCode { get; set; } //this field can have 0 “Success” or a non-zero “Error” code. Error codes can be found at section 1.3

        [JsonProperty("result_description")]
        public string ResultDescription { get; set; } //Detailed info about return code

        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }//String representation of return code
    }
}