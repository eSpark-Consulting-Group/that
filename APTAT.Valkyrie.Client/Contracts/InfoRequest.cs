using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class InfoRequest : RequestBase
    {
        [JsonProperty("sha1")]
        public string SHA1 { get; set;}
    }
}
