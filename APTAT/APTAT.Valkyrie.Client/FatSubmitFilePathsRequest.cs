using Newtonsoft.Json;
using Valkyrie.WebApiProvider.Contracts;

namespace Valkyrie.WebApiProvider
{
    public class FatSubmitFilePathsRequest: RequestBase
    {
        [JsonProperty("scan_id")]
        public string ScanId { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("file_paths")]
        public FilePath[] FilePaths { get; set; }
    }
}