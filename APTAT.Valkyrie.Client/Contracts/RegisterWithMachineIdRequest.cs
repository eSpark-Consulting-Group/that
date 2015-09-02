using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class RegisterWithMachineIdRequest
    {
        [JsonProperty("machine_id")]
        public string MachineId
        {
            get;
            set;
        }
    }
}