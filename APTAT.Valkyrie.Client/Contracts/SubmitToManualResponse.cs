using APTAT.Valkyrie.Client.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class SubmitToManualResponse
    {
        public string manual_analysis_id { get; set; }
        public ErrorCodesEnum  return_code { get; set; }
        public string result_message { get; set; }
        public string result_description { get; set; }
        public string sha1 { get; set; }
    }
}
