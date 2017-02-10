using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Valkyrie.WebApiProvider.Contracts
{
    public class FilePath
    {
        [JsonProperty("machine_name")]

        public string MachineName { get; set; }

        [JsonProperty("file_path")]

        public string Path { get; set; }
    }
}
