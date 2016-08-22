using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
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