using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    /// <summary>
    /// 1.1.2 Register with CAM License (/fvs_register_with_cam_license)
    /// </summary>
    public class RegisterWithCamLicenseRequest
    {
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("product_version")]
        public string ProductVersion { get; set; }


        [JsonProperty("license_id")]
        public string LicenseId
        {
            get;
            set;
        }

        //if registered using register_with_cam_credentials before then there is no need to add this field, it will be automatically associated with existing api key
        [JsonProperty("api_key")]
        public string ApiKey
        {
            get; 
            set;
        }
    }
}