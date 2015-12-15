using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    /// <summary>
    /// 1.1.3 Register with CAM Credentials (/fvs_register_with_cam_credentials)
    /// </summary>
    public class RegisterWithCamCredentials
    {
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("product_version")]
        public string ProductVersion { get; set; }

        [JsonProperty("username")]
        public string UserName
        {
            get;
            set;
        }
        
        [JsonProperty("password")]
        public string Password
        {
            get;
            set;
        }

        //API Key Key, Optional: [api_key]: string (if registered using register_with_cam_license before then there is no need to add this field, it will be automatically associated with existing api key)
        [JsonProperty("api_key")]
        public string ApiKey
        {
            get;
            set;
        }
    }
}