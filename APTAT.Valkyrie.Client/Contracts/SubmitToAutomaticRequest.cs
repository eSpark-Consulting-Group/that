using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class SubmitToAutomaticRequest: RequestBase
    {
        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }

        [JsonProperty("file_data")]
        public string FileData { get; set; }
    }
}
